import mysql.connector
from mysql.connector import Error
import utils.settings as settings


class DatabaseConnection:
    def __init__(self):
        self.conn = self.connect()

    def connect(self):
        try:
            self.conn = mysql.connector.connect(
                host=settings.get_source(),
                database=settings.get_catalog(),
                user=settings.get_username(),
                password=settings.get_password(),
            )
            if self.conn.is_connected():
                db_Info = self.conn.get_server_info()
                print("Connected to MySQL Server version ", db_Info)
            return self.conn
        except Exception as e:
            print("I am unable to connect to the database")
            print(str(e))
            return None

    def close(self):
        self.conn.close()
        if self.conn.is_connected():
            self.cursor.close()
            self.conn.close()
            print("MySQL connection is closed")

    def get_connector(self):
        return self.conn

    def query(self, query):
        cur = self.conn.cursor()
        cur.execute(query)
        return cur.fetchall()
