import socket
import threading

class SocketServer:
    def __init__(self, host="127.0.0.1", port=65432):
        """
        Initializes the SocketServer.
        :param host: IP address to bind the server to.
        :param port: Port to listen on.
        """
        self.host = host
        self.port = port
        self.server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.client_socket = None
        self.client_address = None
        self.running = False

    def start_server(self):
        """
        Starts the server and begins listening for connections.
        """
        try:
            self.server_socket.bind((self.host, self.port))
            self.server_socket.listen(1)
            print(f"Server started on {self.host}:{self.port}. Waiting for a connection...")
            self.running = True
            threading.Thread(target=self._accept_connection, daemon=True).start()
        except Exception as e:
            print(f"Error starting server: {e}")
            self.running = False

    def _accept_connection(self):
        """
        Continuously accept connections from clients.
        """
        while self.running:
            try:
                if not self.client_socket:  # Only accept a new connection if none exists
                    self.client_socket, self.client_address = self.server_socket.accept()
                    print(f"Connection established with {self.client_address}")
            except Exception as e:
                print(f"Error accepting connection: {e}")
                self.stop_connection()

    def send_message(self, message):
        """
        Sends a message to the connected client.
        :param message: Message string to send.
        """
        if self.client_socket:
            try:
                self.client_socket.sendall(message.encode("utf-8"))
            except Exception as e:
                print(f"Error sending message: {e}")
                self.stop_connection()

    def receive_message(self):
        """
        Receives a message from the connected client.
        :return: Decoded message string or None if no message received.
        """
        if self.client_socket:
            try:
                data = self.client_socket.recv(1024)
                if data:
                    return data.decode("utf-8")
            except Exception as e:
                print(f"Error receiving message: {e}")
                self.stop_connection()
        return None

    def stop_connection(self):
        """
        Stops the current client connection.
        """
        if self.client_socket:
            try:
                self.client_socket.close()
                print(f"Connection with {self.client_address} closed.")
            except Exception as e:
                print(f"Error closing connection: {e}")
            finally:
                self.client_socket = None
                self.client_address = None

    def stop_server(self):
        """
        Stops the server and closes all connections.
        """
        self.running = False
        self.stop_connection()
        self.server_socket.close()
        print("Server stopped.")

if __name__ == "__main__":
    server = SocketServer()
    server.start_server()

    try:
        while True:
            if server.client_socket:
                received_message = server.receive_message()
                if received_message:
                    print(f"Received: {received_message}")
                    server.send_message(f"Echo: {received_message}")
    except KeyboardInterrupt:
        print("Shutting down server...")
    finally:
        server.stop_server()
