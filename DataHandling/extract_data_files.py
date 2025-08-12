import os
from pathlib import Path
from dotenv import load_dotenv
from datetime import datetime
import time
import requests
import zipfile
import io
import pandas as pd

# Load environment variables
load_dotenv()
API_TOKEN = os.getenv("API_TOKEN")
SURVEY_IDS = os.getenv("SURVEY_IDS") # split by spaces
DATA_CENTER = os.getenv("DATA_CENTER")
DESTINATION_DIR = os.getenv('DESTINATION_DIR')

survey_id_list = SURVEY_IDS.split(" ")

script_dir = Path(__file__).resolve().parent
dest_dir = (script_dir / DESTINATION_DIR).resolve()
dest_dir.mkdir(parents=True, exist_ok=True)

# Set request headers
headers = {
    "x-api-token": API_TOKEN,
    "content-type": "application/json"
}

def file_ts():
    return datetime.now().strftime("%Y%m%d%H%M%S")

def export_single_survey(survey_id: str, max_poll_seconds: int = 300, poll_interval: float = 2.0):
    export_url = f"https://{DATA_CENTER}.qualtrics.com/API/v3/surveys/{survey_id}/export-responses/"
    try:
        # Step 1: Start export request
        print(f"[{survey_id}] Requesting data export...")
        response = requests.post(export_url, headers=headers, json={"format": "csv"}, timeout=30)
        if not response:
            print(f"[{survey_id}] Start export failed: {response.status_code} {response.text}")
            return

        progress_id = response.json()["result"]["progressId"]
        if not progress_id:
            print(f"[{survey_id}] No progressId in response.")
            return

        # Step 2: Poll for export status
        file_id = None
        print("Waiting for export to complete...")
        waited = 0
        while waited < max_poll_seconds:
            check_response = requests.get(export_url + progress_id, headers=headers)
            result = check_response.json()["result"]
            if result["status"] == "complete":
                file_id = result["fileId"]
                break
            elif result["status"] in {"failed", "error"}:
                print(f"[{survey_id}] Export failed: {result}")
                return
            time.sleep(poll_interval)
            waited += poll_interval

        if not file_id:
            print(f"[{survey_id}] Export timed out.")
            return

        # Step 3: Download and extract the file
        print(f"[{survey_id}] Downloading...")
        download_url = export_url + file_id + "/file"
        download_response = requests.get(download_url, headers=headers)
        if not download_response.ok:
            print(f"[{survey_id}] Download failed: {download_response.status_code}")
            return

        with zipfile.ZipFile(io.BytesIO(download_response.content)) as zip_file:
            filename = zip_file.namelist()[0]
            with zip_file.open(filename) as f:
                df = pd.read_csv(f)

        # Step 4: Save to local path
        file_name = f"qualtrics_data_{survey_id}_{file_ts()}.csv"
        dest_path = dest_dir / file_name
        df.to_csv(dest_path, index=False)
        print(f"[{survey_id}] Data saved to {dest_path}")

    except requests.exceptions.Timeout:
        print(f"[{survey_id}] Network timeout.")
    except requests.exceptions.RequestException as e:
        print(f"[{survey_id}] Request error: {e}")
    except zipfile.BadZipFile:
        print(f"[{survey_id}] Bad ZIP file received.")
    except Exception as e:
        print(f"[{survey_id}] Unexpected error: {e}")


if __name__ == "__main__":
    print(f"Surveys to export: {', '.join(survey_id_list)}")
    for sid in survey_id_list:
        export_single_survey(sid)
    print("All done.")
