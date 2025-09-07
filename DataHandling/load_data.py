import os
from dotenv import load_dotenv
import psycopg2

# load environment variables
load_dotenv()

# Fetch variables
DB_USER = os.getenv("DB_USER")
DB_PASSWORD = os.getenv("DB_PASSWORD")
DB_HOST = os.getenv("DB_HOST")
DB_PORT = os.getenv("DB_PORT")
DB_NAME = os.getenv("DB_NAME")

# Connect to the database
try:
    conn = psycopg2.connect(
        user=DB_USER,
        password=DB_PASSWORD,
        host=DB_HOST,
        port=DB_PORT,
        dbname=DB_NAME,
        sslmode="require"
    )
    cur = conn.cursor()
    cur.execute("SELECT version(), current_timestamp;")
    print("Connected OK:", cur.fetchone())
    cur.close()
    conn.close()

except Exception as e:
    print("Connection failed:", e)