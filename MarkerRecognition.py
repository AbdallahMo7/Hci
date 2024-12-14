from pythonosc import dispatcher, osc_server
import threading

class TUIOReceiver:
    def __init__(self, host='127.0.0.1', port=3333):
        """
        Initializes the TUIO Receiver.
        :param host: Host IP address to listen on.
        :param port: Port number to listen on.
        """
        self.host = host
        self.port = port
        self.received_data = []  # List to store TUIO messages

        # Set up the dispatcher for OSC message handling
        self.message_dispatcher = dispatcher.Dispatcher()
        self.message_dispatcher.map("/tuio/2Dobj", self._process_object_message)

    def _process_object_message(self, address, *values):
        """
        Handles incoming TUIO object messages.
        :param address: The OSC address of the message.
        :param values: Message data received.
        """
        message_type = values[0]
        if message_type == "set":
            object_info = {
                "category": "object",
                "session": values[1],
                "identifier": values[2],
                "x_coordinate": values[3],
                "y_coordinate": values[4],
                "orientation": values[5],
            }
            self.received_data.append(object_info)
        elif message_type == "alive":
            active_ids = values[1:]
            self.received_data.append({"category": "alive", "active_ids": list(active_ids)})

    def run_server(self):
        """
        Starts the OSC server and begins listening for TUIO messages.
        """
        osc_server_instance = osc_server.ThreadingOSCUDPServer((self.host, self.port), self.message_dispatcher)
        server_thread = threading.Thread(target=osc_server_instance.serve_forever, daemon=True)
        server_thread.start()
        print(f"TUIO Receiver active on {self.host}:{self.port}")

    def fetch_data(self):
        """
        Retrieves all collected TUIO messages and clears the buffer.
        :return: List of dictionaries containing TUIO messages.
        """
        current_data = self.received_data[:]
        self.received_data.clear()
        return current_data

# Example usage
if __name__ == "__main__":
    tuio_receiver = TUIOReceiver(host='127.0.0.1', port=3333)
    tuio_receiver.run_server()

    try:
        while True:
            tuio_messages = tuio_receiver.fetch_data()
            if tuio_messages:
                print("TUIO Messages:")
                for msg in tuio_messages:
                    if msg["category"] == "object":
                        print(f"Object ID: {msg['identifier']}, Position: ({msg['x_coordinate']}, {msg['y_coordinate']}), Orientation: {msg['orientation']}")
                    elif msg["category"] == "alive":
                        print(f"Active IDs: {msg['active_ids']}")
           
    except KeyboardInterrupt:
        print("TUIO Receiver stopped.")
