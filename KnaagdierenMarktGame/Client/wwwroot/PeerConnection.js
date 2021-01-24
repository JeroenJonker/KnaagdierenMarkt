var lastPeerId = null;
var peer = null;
var peerId = null;
var connections = [null, null, null, null, null, null, null, null];
var applicationName = 'KnaagdierenMarktGame.Client';

// get available spot in connections to store the connection
function GetNextAvailableConnection() {
    var i;
    for (i = 0; i < 4; i++) {
        if (connections[i] == null) {
            return i;
        }
    }
    return -1;
}

function GetConnectionPositionFromID(id) {
    var i;
    for (i = 0; i < 4; i++) {
        if (connections[i] != null && connections[i].peer == id) {
            console.log('is already connected...');
            return i;
        }
    }
    return -1;
}

function initializeConnection() {

    peer = new Peer(null, {
        debug: 2
    });
    //peer = new Peer('', {
    //    host: 'localhost',
    //    port: 9000,
    //    path: '/myapp'
    //});

    peer.on('open', function (id) {
        // Workaround for peer.reconnect deleting previous id
        if (peer.id === null) {
            console.log('Received null id from peer open');
            peer.id = lastPeerId;
        } else {
            lastPeerId = peer.id;
        }
        DotNet.invokeMethodAsync(applicationName, 'SetPeerID', peer.id);
        console.log('ID: ' + peer.id);
    });
    peer.on('connection', function (c) {
        var nextAvailableConnectionNumber = GetNextAvailableConnection();
        var connectionPosition = GetConnectionPositionFromID(c.peer);
        if (connectionPosition > -1)
        {
            connections[connectionPosition] = c;
        }
        else if (nextAvailableConnectionNumber > -1 && connectionPosition == -1) {
            connections[nextAvailableConnectionNumber] = c;
            console.log("Connected to: " + connections[nextAvailableConnectionNumber].peer);
        }
        else {
            c.send("Already enough connected clients");
            setTimeout(function () { c.close(); }, 500);
        }
    });
    peer.on('disconnected', function () {
        console.log('Connection lost. Please reconnect');

        // Workaround for peer.reconnect deleting previous id
        peer.id = lastPeerId;
        peer._lastServerId = lastPeerId;
        peer.reconnect();
    });
    peer.on('close', function () { 
        console.log('Connection destroyed');
    });
    peer.on('error', function (err) {
        console.log(err);
        alert('' + err);
    });
}

function join(recvIdInput) {
    var NextAvailableConnectionNumber = GetNextAvailableConnection();
    if (NextAvailableConnectionNumber > -1) {
        var connectNumber = NextAvailableConnectionNumber;
        var connectionPosition = GetConnectionPositionFromID(recvIdInput);
        if (connectionPosition > -1) {
            connectNumber = connectionPosition;
        }
        else {
            connections[connectNumber] = peer.connect(recvIdInput, {
                reliable: true
            });

            connections[connectNumber].on('open', function () {
                console.log("Connected to: " + connections[connectNumber].peer);
            });
        }

        connections[connectNumber].on('data', function (data) {
            console.log("Data recieved");
            DotNet.invokeMethodAsync(applicationName, 'UpdateMessageCaller', data)
        });
        connections[connectNumber].on('close', function () {
            console.log("Connection closed");
        });
    }
};

function sendMessageToPeer(message, connection) {
    if (connection && connection.open) {
        connection.send(message);
        //console.log(sigName + " signal sent");
    } else {
        console.log('Connection is closed');
    }
}

function sendMessageToAllPeers(message) {
    var i;
    for (i = 0; i < 4; i++) {
        if (connections[i] != null) {
            console.log('send message...');
            sendMessageToPeer(message, connections[i]);
        }
    }
}
