import pandas as pd
import logging
from typing import Dict, Any

from ..config.settings import get_config
from ..utils.file_utils import find_latest_csv
from ..config.database import db_manager

logger = logging.getLogger(__name__)


class DataTransformService:
    def __init__(self):
        self.config = get_config()

        self.key_fields = ["Facility", "Satisfaction", "EndDate", "NPS", "NPS_NPS_GROUP", "Gender", "ParticipantType"]
        self.key_fields_prefixes = ["Ab_"]
        self.allowed_keys_dict = ["ServiceType", "Facility", "Satisfaction", "Gender", "ParticipantType"]
        self.allowed_prefixes = ["Ab_"]

    def transform_survey_mappings(self, survey_id: str, questions: Dict[str, Any]):
        try:
            if not questions or (isinstance(questions, dict) and len(questions) == 0):
                logger.info(f"[{survey_id}] No questions provided from extract stage, skip mappings transform.")
                return {
                    "success": True,
                    "survey_id": survey_id,
                    "action": "skipped",
                    "reason": "no_questions_provided",
                    "mappings_data": {"mappings": {}, "key_fields": {}},
                    "mappings_count": 0,
                    "key_fields_count": 0
                }

            logger.info(f"[{survey_id}] Transforming mappings")
            mappings_data = self._extract_mappings_from_questions(questions)

            return {
                "success": True,
                "survey_id": survey_id,
                "mappings_data": mappings_data,
                "mappings_count": len(mappings_data.get("mappings", {})),
                "key_fields_count": len(mappings_data.get("key_fields", {}))
            }

        except Exception as e:
            logger.error(f"[{survey_id}] Failed to transform mappings: {e}")
            return {"success": False, "error": str(e)}

    def transform_survey_responses(self, survey_id: str):
        try:
            dup_check = self._is_latest_duplicate_download(survey_id)
            if dup_check.get("is_duplicate"):
                logger.info(f"[{survey_id}] Latest download hash equals previous one; skip transform & load.")
                return {
                    "success": True,
                    "survey_id": survey_id,
                    "action": "skipped_duplicate",
                    "reason": "latest_two_file_hash_equal",
                    "transformed_count": 0,
                    "responses_data": [],
                    "total_records_in_csv": 0,
                    "hash": dup_check.get("latest_hash"),
                }

            logger.info(f"[{survey_id}] Transforming responses")

            csv_file = find_latest_csv(self.config.DATA_DIR, survey_id)
            df_responses = pd.read_csv(csv_file)

            responses_data = self._transform_responses_data(df_responses)

            return {
                "success": True,
                "survey_id": survey_id,
                "transformed_count": len(responses_data),
                "responses_data": responses_data,
                "total_records_in_csv": len(df_responses)
            }

        except FileNotFoundError:
            error_msg = f"CSV file not found for survey {survey_id}"
            logger.error(f"[{survey_id}] {error_msg}")
            return {"success": False, "error": error_msg}
        except Exception as e:
            logger.error(f"[{survey_id}] Failed to transform responses: {e}")
            return {"success": False, "error": str(e)}

    def _is_latest_duplicate_download(self, survey_id: str) -> dict:
        try:
            with db_manager.get_cursor() as cursor:
                cursor.execute(
                    """
                    SELECT file_hash, extracted_at
                    FROM survey_responses_extraction_log
                    WHERE survey_id = %s
                    ORDER BY extracted_at DESC LIMIT 2
                    """,
                    (survey_id,)
                )
                rows = cursor.fetchall() or []

            if len(rows) < 2:
                return {"is_duplicate": False}

            latest_hash = rows[0]["file_hash"]
            prev_hash = rows[1]["file_hash"]

            if latest_hash and prev_hash and latest_hash == prev_hash:
                return {"is_duplicate": True, "latest_hash": latest_hash}

            return {"is_duplicate": False}
        except Exception as e:
            logger.warning(f"[{survey_id}] Failed to check duplicate download, will proceed with transform. Error: {e}")
            return {"is_duplicate": False}

    def transform_all(self, survey_ids):
        if not survey_ids:
            return {"success": False, "error": "No survey IDs provided"}

        return {"success": False, "error": "transform_all requires explicit questions per survey; orchestrate upstream."}

    def _extract_mappings_from_questions(self, questions):
        transformed_fields = {
            "key_fields": {},
            "mappings": {}
        }

        for question in questions.values():
            outer_key = question.get("DataExportTag")
            if not outer_key:
                continue

            if (outer_key not in self.allowed_keys_dict and
                    not any(outer_key.startswith(p) for p in self.allowed_prefixes)):
                continue

            choices = question.get("Choices") or {}
            if choices:
                inner_mapping = {}
                for key, value in choices.items():
                    # value 可能是 {"Display": "..."} 结构
                    display = value.get("Display") if isinstance(value, dict) else str(value)
                    inner_mapping[key] = display

                if outer_key == "ServiceType":
                    transformed_fields["key_fields"][outer_key] = inner_mapping.get("1", "")
                else:
                    transformed_fields["mappings"][outer_key] = inner_mapping

        return transformed_fields

    def _transform_responses_data(self, df):
        prefix_cols = [col for col in df.columns
                       if any(col.startswith(p) for p in self.key_fields_prefixes)]
        available_cols = [col for col in (self.key_fields + prefix_cols)
                          if col in df.columns]

        df_selected = df[available_cols]

        data = df_selected.to_dict(orient='records')[2:]
        return data