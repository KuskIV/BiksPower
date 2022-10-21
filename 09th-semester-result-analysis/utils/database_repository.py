from utils.database_connection import DatabaseConnection


class DataRepository:
    def __init__(self):
        self.conn = DatabaseConnection()

    def query(self, query):
        return self.conn.query(query)
