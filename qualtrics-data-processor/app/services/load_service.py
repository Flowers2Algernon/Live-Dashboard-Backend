import json
import pandas as pd
import logging
from datetime import datetime

from ..config.database import db_manager

logger = logging.getLogger(__name__)


class DataLoadService:
    def __init__(self):
        pass

    def load_survey_mappings(self, survey_id, mappings_data, force_update=False):
        try:
            logger.info(f"Loading mappings for survey {survey_id}")

            updated = self._update_survey_mappings_by_qualtrics_id(survey_id, mappings_data)

            if updated:
                return {
                    "success": True,
                    "action": "updated",
                    "mappings_count": len(mappings_data.get("mappings", {})),
                    "key_fields_count": len(mappings_data.get("key_fields", {}))
                }
            else:
                # 没有匹配到任何行，通常是 surveys 表不存在该 qualtrics_survey_id
                return {
                    "success": False,
                    "action": "failed",
                    "error": f"No surveys row matched qualtrics_survey_id={survey_id}"
                }

        except Exception as e:
            logger.error(f"Failed to load mappings for survey {survey_id}: {e}")
            return {"success": False, "error": str(e), "action": "failed"}

    def load_survey_responses(self, survey_id, responses_data, replace_existing=True):
        try:
            logger.info(f"Loading responses for survey {survey_id}")

            survey_uuid = self._get_survey_uuid_by_qualtrics_id(survey_id)
            if not survey_uuid:
                return {
                    "success": False,
                    "error": f"Survey with qualtrics_survey_id {survey_id} not found in database"
                }

            deleted_count = 0
            if replace_existing:
                deleted_count = self._clear_survey_responses(survey_uuid)

            inserted_count = self._insert_survey_responses(survey_uuid, responses_data)

            return {
                "success": True,
                "deleted_count": deleted_count,
                "inserted_count": inserted_count,
                "total_input_records": len(responses_data) if responses_data else 0
            }

        except Exception as e:
            logger.error(f"Failed to load responses for survey {survey_id}: {e}")
            return {
                "success": False,
                "error": str(e)
            }

    def _get_survey_uuid_by_qualtrics_id(self, qualtrics_survey_id):
        try:
            with db_manager.get_cursor() as cursor:
                query = "SELECT id FROM surveys WHERE qualtrics_survey_id = %s"
                cursor.execute(query, (qualtrics_survey_id,))
                result = cursor.fetchone()
                if result:
                    return result['id']
                else:
                    logger.warning(f"Survey with qualtrics_survey_id {qualtrics_survey_id} not found")
                    return None
        except Exception as e:
            logger.error(f"Failed to get survey UUID: {e}")
            raise

    def _has_existing_mappings(self, survey_uuid):
        try:
            with db_manager.get_cursor() as cursor:
                query = """
                        SELECT field_mapping
                        FROM surveys
                        WHERE id = %s
                          AND field_mapping IS NOT NULL
                          AND field_mapping != '{}'::jsonb
                        """
                cursor.execute(query, (survey_uuid,))
                result = cursor.fetchone()
                return result is not None
        except Exception as e:
            logger.error(f"Failed to check existing mappings: {e}")
            raise

    def _update_survey_mappings_by_qualtrics_id(self, qualtrics_survey_id, mappings_data) -> bool:
        try:
            with db_manager.get_cursor() as cursor:
                combined_mapping = {
                    "field_mappings": mappings_data.get("mappings", {}),
                    "key_fields": mappings_data.get("key_fields", {}),
                    "updated_at": datetime.now().isoformat()
                }

                update_query = """
                    UPDATE surveys
                    SET field_mapping = %s
                    WHERE qualtrics_survey_id = %s
                """
                cursor.execute(update_query, (json.dumps(combined_mapping), qualtrics_survey_id))

                if cursor.rowcount and cursor.rowcount > 0:
                    logger.info(f"[{qualtrics_survey_id}] field_mapping updated (rows={cursor.rowcount})")
                    return True
                else:
                    logger.warning(f"[{qualtrics_survey_id}] field_mapping not updated (no rows matched)")
                    return False
        except Exception as e:
            logger.error(f"Failed to update survey mappings by qualtrics id {qualtrics_survey_id}: {e}")
            return False

    def _clear_survey_responses(self, survey_uuid):
        try:
            with db_manager.get_cursor() as cursor:
                delete_query = "DELETE FROM survey_responses WHERE survey_id = %s"
                cursor.execute(delete_query, (survey_uuid,))
                deleted_count = cursor.rowcount
                logger.info(f"Deleted {deleted_count} existing responses for survey {survey_uuid}")
                return deleted_count
        except Exception as e:
            logger.error(f"Failed to clear survey responses: {e}")
            raise

    def _insert_survey_responses(self, survey_uuid, responses_data):
        if not responses_data:
            logger.warning("No response data to insert")
            return 0

        try:
            with db_manager.get_cursor() as cursor:
                insert_query = """
                               INSERT INTO survey_responses
                               (survey_id, qualtrics_response_id, submitted_at, period_year, period_month,
                                response_data)
                               VALUES (%s, %s, %s, %s, %s, %s)
                               """

                inserted_count = 0
                for idx, response in enumerate(responses_data):
                    try:
                        qualtrics_response_id = response.get('ResponseId', f"generated_{survey_uuid}_{idx}")

                        submitted_at = None
                        if 'EndDate' in response and response['EndDate']:
                            try:
                                submitted_at = pd.to_datetime(response['EndDate'])
                            except:
                                submitted_at = None

                        period_year = None
                        period_month = None
                        if submitted_at:
                            period_year = submitted_at.year
                            period_month = submitted_at.replace(day=1).date()

                        cursor.execute(insert_query, (
                            survey_uuid,
                            qualtrics_response_id,
                            submitted_at,
                            period_year,
                            period_month,
                            json.dumps(response)
                        ))
                        inserted_count += 1

                    except Exception as row_error:
                        logger.warning(f"Failed to insert response {idx}: {row_error}")
                        continue

                logger.info(f"Successfully inserted {inserted_count} responses using survey UUID {survey_uuid}")
                return inserted_count

        except Exception as e:
            logger.error(f"Failed to insert survey responses: {e}")
            raise