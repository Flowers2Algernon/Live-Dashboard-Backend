#!/usr/bin/env python3
"""
Database Connection Test Script
用于测试PostgreSQL数据库连接是否正常
"""

import os
import sys
import psycopg2
from psycopg2.extras import RealDictCursor
from dotenv import load_dotenv

# 加载环境变量
load_dotenv()


def test_database_connection():
    """测试数据库连接"""

    # 获取数据库配置
    db_config = {
        'host': os.getenv('DB_HOST'),
        'port': int(os.getenv('DB_PORT', 5432)),
        'database': os.getenv('DB_NAME'),
        'user': os.getenv('DB_USER'),
        'password': os.getenv('DB_PASSWORD')
    }

    print("=== Database Connection Test ===")
    print(f"Host: {db_config['host']}")
    print(f"Port: {db_config['port']}")
    print(f"Database: {db_config['database']}")
    print(f"User: {db_config['user']}")
    print(f"Password: {'*' * len(db_config['password']) if db_config['password'] else 'Not set'}")
    print()

    # 检查必需的环境变量
    missing_vars = []
    for key, value in db_config.items():
        if not value:
            missing_vars.append(key.upper().replace('DATABASE', 'DB_NAME'))

    if missing_vars:
        print(f"❌ Missing environment variables: {', '.join(missing_vars)}")
        print("Please check your .env file")
        return False

    # 测试基本连接
    print("Testing basic connection...")
    try:
        conn = psycopg2.connect(**db_config)
        print("✅ Basic connection successful")
        conn.close()
    except psycopg2.OperationalError as e:
        print(f"❌ Basic connection failed: {e}")
        return False
    except Exception as e:
        print(f"❌ Unexpected error: {e}")
        return False

    # 测试带编码设置的连接
    print("Testing connection with encoding...")
    try:
        conn = psycopg2.connect(
            **db_config,
            client_encoding='UTF8',
            connect_timeout=30,
            application_name='connection_test'
        )
        print("✅ Connection with encoding successful")

        # 测试查询
        print("Testing query execution...")
        cursor = conn.cursor(cursor_factory=RealDictCursor)
        cursor.execute("SELECT version() as pg_version, current_database() as db_name")
        result = cursor.fetchone()
        print(f"✅ Query successful")
        print(f"PostgreSQL Version: {result['pg_version']}")
        print(f"Current Database: {result['db_name']}")

        cursor.close()
        conn.close()

    except Exception as e:
        print(f"❌ Connection with encoding failed: {e}")
        return False

    # 测试表是否存在
    print("Checking required tables...")
    try:
        conn = psycopg2.connect(
            **db_config,
            client_encoding='UTF8'
        )
        cursor = conn.cursor()

        required_tables = ['surveys', 'survey_responses', 'survey_responses_extraction_log']
        for table in required_tables:
            cursor.execute("""
                           SELECT EXISTS (SELECT
                                          FROM information_schema.tables
                                          WHERE table_schema = 'public'
                                            AND table_name = %s)
                           """, (table,))
            exists = cursor.fetchone()[0]
            status = "✅" if exists else "❌"
            print(f"{status} Table '{table}': {'exists' if exists else 'missing'}")

        cursor.close()
        conn.close()

    except Exception as e:
        print(f"❌ Table check failed: {e}")
        return False

    print("\n=== Connection Test Complete ===")
    return True


def test_connection_pool():
    """测试连接池"""
    print("\n=== Testing Connection Pool ===")

    try:
        from psycopg2.pool import ThreadedConnectionPool

        db_config = {
            'host': os.getenv('DB_HOST'),
            'port': int(os.getenv('DB_PORT', 5432)),
            'database': os.getenv('DB_NAME'),
            'user': os.getenv('DB_USER'),
            'password': os.getenv('DB_PASSWORD'),
            'client_encoding': 'UTF8',
            'cursor_factory': RealDictCursor
        }

        pool = ThreadedConnectionPool(
            minconn=1,
            maxconn=3,
            **db_config
        )

        print("✅ Connection pool created successfully")

        # 测试从池中获取连接
        conn1 = pool.getconn()
        print("✅ Got connection from pool")

        # 测试查询
        cursor = conn1.cursor()
        cursor.execute("SELECT 1 as test")
        result = cursor.fetchone()
        assert result['test'] == 1
        print("✅ Query through pool successful")

        # 归还连接
        pool.putconn(conn1)
        print("✅ Returned connection to pool")

        # 关闭池
        pool.closeall()
        print("✅ Connection pool closed")

        return True

    except Exception as e:
        print(f"❌ Connection pool test failed: {e}")
        return False


if __name__ == "__main__":
    print("Starting database connection tests...\n")

    # 基本连接测试
    basic_test = test_database_connection()

    # 连接池测试
    pool_test = test_connection_pool()

    print(f"\n=== Test Results ===")
    print(f"Basic Connection: {'✅ PASS' if basic_test else '❌ FAIL'}")
    print(f"Connection Pool: {'✅ PASS' if pool_test else '❌ FAIL'}")

    if basic_test and pool_test:
        print("\n🎉 All tests passed! Your database connection should work.")
        sys.exit(0)
    else:
        print("\n❌ Some tests failed. Please check your database configuration.")
        sys.exit(1)