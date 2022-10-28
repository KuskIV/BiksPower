from utils.database_connection import DatabaseConnection


class DataRepository:
    def __init__(self):
        self.conn = DatabaseConnection()

    def query_all(self, query, data_tuple=()):
        return self.conn.query_all(query, data_tuple)

    def query_one(self, query, data_tuple=()):
        return self.conn.query_one(query, data_tuple)

    def close(self):
        self.conn.close()
