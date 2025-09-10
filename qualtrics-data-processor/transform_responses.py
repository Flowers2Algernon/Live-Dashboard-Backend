import json
import os
from pathlib import Path
from dotenv import load_dotenv
import re
import pandas as pd

# Load environment variables
load_dotenv()
SURVEY_IDS = os.getenv("SURVEY_IDS") # split by spaces

script_dir = Path(__file__).resolve().parent
base = script_dir / "data"

key_fields = ["Facility", "Satisfaction", "EndDate", "NPS", "NPS_NPS_GROUP", "Gender", "ParticipantType"]
key_fields_prefixes = ["Ab_"]

def find_latest_csv(base_dir, survey_id):
    ts_regex = r'_(\d{14})$'

    candidates = []
    for path in base_dir.glob(f"*{survey_id}*.csv"):
        matched = re.search(ts_regex, path.stem)
        if not matched:
            continue
        ts_str = matched.group(1)
        candidates.append((ts_str, path))

    if not candidates:
        raise FileNotFoundError(f"No {survey_id} csv files found in {base_dir}")

    return max(candidates, key=lambda x: x[0])[1]


def load_csv(survey_id, base_dir):
    return pd.read_csv(find_latest_csv(base_dir, survey_id))


def transform_key_values(df, columns):
    if key_fields_prefixes:
        prefix_cols = [col for col in df.columns if any(col.startswith(p) for p in key_fields_prefixes)]
    else:
        prefix_cols = []

    complete_cols = columns + prefix_cols

    df_selected = df[complete_cols]
    data = df_selected.to_dict(orient='records')[2:] # Remove the headers
    return data


if __name__ == "__main__":
    survey_id_list = SURVEY_IDS.split(" ")
    key_values_by_survey = {}
    for survey_id in survey_id_list:
        df_responses = load_csv(survey_id, base)
        key_values_by_survey[survey_id] = transform_key_values(df_responses, key_fields)

    # Convert dict to json
    key_values_by_survey_json = json.dumps(key_values_by_survey)
    print(key_values_by_survey_json)