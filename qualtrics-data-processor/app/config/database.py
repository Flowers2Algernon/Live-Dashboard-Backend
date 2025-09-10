from contextlib import contextmanager
import logging
from psycopg2.pool import ThreadedConnectionPool
from psycopg2.extras import RealDictCursor
from settings import get_config

# Logging
logger = logging.getLogger(__name__)

class DatabaseManager:
    def __init__(self):
        self.connection_pool = None
        self._init_connection_pool()
        self.config = get_config()

    def _init_connection_pool(self):
        try:
            self.connection_pool = ThreadedConnectionPool(
                minconn=int(self.config.QUALTRICS_API_MIN_CONNECTIONS),
                maxconn=int(self.config.QUALTRICS_API_MAX_CONNECTIONS),
                host=self.config.DB_HOST,
                port=int(self.config.DB_PORT),
                database=self.config.DB_DATABASE,
                user=self.config.DB_USER,
                password=self.config.DB_PASSWORD,
                cursor_factory=RealDictCursor,
                options="-c default_transaction_isolation=read_committed"
            )
            logger.info("Database connection pool initialized successfully")
        except Exception as e:
            logger.error(f"Failed to initialize database connection pool: {e}")
            raise

    @contextmanager
    def get_connection(self):
        conn = None
        try:
            conn = self.connection_pool.getconn()
            yield conn
        except Exception as e:
            if conn:
                conn.rollback()
            logger.error(f"Database operation failed: {e}")
            raise
        finally:
            if conn:
                self.connection_pool.putconn(conn)

    @contextmanager
    def get_cursor(self, autocommit=False):
        with self.get_connection() as conn:
            if autocommit:
                conn.autocommit = True
            try:
                cursor = conn.cursor()
                yield cursor
                if not autocommit:
                    conn.commit()
            except Exception as e:
                if not autocommit:
                    conn.rollback()
                raise
            finally:
                if not autocommit:
                    conn.autocommit = False

    def close_all_connections(self):
        if self.connection_pool:
            self.connection_pool.closeall()
            logger.info("All database connections closed")


db_manager = DatabaseManager()