import os
from pathlib import Path
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

class Config:
    # Flask
    SECRET_KEY = os.getenv('SECRET_KEY')
    JSON_AS_ASCII = False
    JSONIFY_PRETTYPRINT_REGULAR = True

    # App settings
    APP_NAME = "Qualtrics Data Processor"
    APP_VERSION = "1.0.0"

    # Qualtrics APIs
    QUALTRICS_API_TOKEN = os.getenv('QUALTRICS_API_TOKEN')
    QUALTRICS_DATA_CENTER = os.getenv('QUALTRICS_DATA_CENTER')

    # Database (Supabase)
    DB_HOST = os.getenv('DB_HOST')
    DB_PORT = os.getenv('DB_PORT')
    DB_DATABASE = os.getenv('DB_DATABASE')
    DB_USER = os.getenv('DB_USER')
    DB_PASSWORD = os.getenv('DB_PASSWORD')

    # Connection Pool
    DB_POOL_MIN_CONN = int(os.getenv('DB_POOL_MIN_CONN'))
    DB_POOL_MAX_CONN = int(os.getenv('DB_POOL_MAX_CONN'))

    # File storage
    BASE_DIR = Path(__file__).resolve().parent.parent.parent
    DATA_DIR = BASE_DIR / 'data'

    DATA_DIR.mkdir(parents=True, exist_ok=True)

    # API
    API_TIMEOUT = int(os.getenv("API_TIMEOUT"))
    EXPORT_POLL_MAX_SECONDS = int(os.getenv("EXPORT_POLL_MAX_SECONDS"))
    EXPORT_POLL_INTERVAL = float(os.getenv("EXPORT_POLL_INTERVAL"))

    # Logging
    LOG_LEVEL = os.getenv('LOG_LEVEL')
    LOG_FORMAT = '%(asctime)s - %(name)s - %(levelname)s - %(message)s'

    @property
    def database_url(self):
        return f"postgresql://{self.DB_USER}:{self.DB_PASSWORD}@{self.DB_HOST}:{self.DB_PORT}/{self.DB_DATABASE}"

class DevelopmentConfig(Config):
    DEBUG = True
    FLASK_ENV = 'development'

class ProductionConfig(Config):
    DEBUG = False
    FLASK_ENV = 'production'

class TestingConfig(Config):
    TESTING = True
    DEBUG = True
    DATABASE_URL = 'sqlite:///:memory:'

config_map = {
    'development': DevelopmentConfig,
    'production': ProductionConfig,
    'testing': TestingConfig,
    'default': DevelopmentConfig
}

def get_config():
    env = os.getenv('FLASK_ENV', 'default')
    return config_map.get(env, DevelopmentConfig)