import os
from dotenv import load_dotenv
import requests
import json

# Load environment variables
load_dotenv()
API_TOKEN = os.getenv("API_TOKEN")
SURVEY_IDS = os.getenv("SURVEY_IDS") # split by spaces
DATA_CENTER = os.getenv("DATA_CENTER")

survey_id_list = SURVEY_IDS.split(" ")

# Set request headers
headers = {
    "x-api-token": API_TOKEN,
    "content-type": "application/json"
}

allowed_keys_dict = ["ServiceType", "Facility", "Satisfaction", "Gender", "ParticipantType"] # “NPS_NPS_GROUP” not found and "NPS" returns list instead of dict
allowed_prefixes = ["Ab_"]
transformed_fields = {
    "key_fields": {},
    "mappings": {}
}

def get_questions_single_survey(survey_id):
    get_survey_url = f"https://{DATA_CENTER}.qualtrics.com/API/v3/survey-definitions/{survey_id}"
    response = requests.get(get_survey_url, headers=headers)
    questions = response.json()["result"]["Questions"]
    return questions

def get_code_label_mappings_n_key_fields(questions):
    for question in questions.values():
        outer_key = question["DataExportTag"]
        if outer_key not in allowed_keys_dict and not any(outer_key.startswith(p) for p in allowed_prefixes):
            continue
        else:
            inner_mapping = {}
            for key, value in question["Choices"].items():
                inner_mapping[key] = value["Display"]
            if outer_key == "ServiceType":
                transformed_fields["key_fields"][outer_key] = inner_mapping["1"]
            else:
                transformed_fields["mappings"][outer_key] = inner_mapping

    return transformed_fields

if __name__ == "__main__":
    mappings_n_key_values_by_survey = {}
    for survey_id in survey_id_list:
        questions = get_questions_single_survey(survey_id)
        transformed_fields_survey = get_code_label_mappings_n_key_fields(questions)
        mappings_n_key_values_by_survey[survey_id] = transformed_fields_survey

    # Convert dict to json
    mappings_n_key_values_by_survey_json = json.dumps(mappings_n_key_values_by_survey)
    print(mappings_n_key_values_by_survey_json)