import os
# from pathlib import Path
from dotenv import load_dotenv
import requests
import json

# Load environment variables
load_dotenv()
API_TOKEN = os.getenv("API_TOKEN")
SURVEY_IDS = os.getenv("SURVEY_IDS") # split by spaces
DATA_CENTER = os.getenv("DATA_CENTER")

# Set request headers
headers = {
    "x-api-token": API_TOKEN,
    "content-type": "application/json"
}

allowed_keys = ["Facility", "Satisfaction", "Ab_"] # “NPS_NPS_GROUP” not found and "NPS" returns list instead of dict
allowed_prefixes = ["Ab_"]
loaded_fields = {}

def get_questions_single_survey(survey_id):
    get_survey_url = f"https://{DATA_CENTER}.qualtrics.com/API/v3/survey-definitions/{survey_id}"
    response = requests.get(get_survey_url, headers=headers)
    questions = response.json()["result"]["Questions"]

    for question in questions.values():
        outer_key = question["DataExportTag"]
        if outer_key not in allowed_keys and not any(outer_key.startswith(p) for p in allowed_prefixes):
            continue
        else:
            inner_mapping = {}
            for key, value in question["Choices"].items():
                inner_mapping[key] = value["Display"]

            if not any(outer_key.startswith(p) for p in allowed_prefixes):
                loaded_fields[outer_key] = inner_mapping
            else:
                loaded_fields[outer_key[3:]] = inner_mapping

    loaded_fields_json = json.dumps(loaded_fields)
    print(loaded_fields_json)

if __name__ == "__main__":
    get_questions_single_survey(SURVEY_IDS)
