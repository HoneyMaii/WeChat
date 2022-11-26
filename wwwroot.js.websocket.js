var server = "wss://localhost:7090"; // 若开启了https 则这里是 wss
var webSocket = new WebSocket(server + "/ws");
let setIntervalWesocketPush = null

// 后台向前台发送消息，前台成功接收后会触发此事件
webSocket.onopen = function (event) {
    console.log("Connection opened...");
    $("#msgList").val("WebSocket connection opened");
}

//webSocket.onerror = function (event) {
//    webSocket.close();
//    clearInterval(setIntervalWesocketPush)
//    console.log('连接失败重连中')
//    if (webSocket.readyState !== 3) {
//        webSocket = null
//        webSocket=new WebSocket(server + "/ws");
//    }
//}

// 后台向前台发送消息，前台成功接收后会触发此事件
webSocket.onmessage = function (event) {
    console.log("Received message: " + event.data);
    if (event.data) {
        var content = $('#msgList').val();
        content = content + '\r\n' + event.data;
        $('#msgList').val(content);
    }
}

// 后台关闭连接后/前台关闭连接后都会触发此事件
webSocket.onclose = function (event) {
    console.log("Connection closed2...");
    var content = $('#msgList').val();
    content = content + '\r\nWebSocket connection closed';
    $('#msgList').val(content);
}

$('#btnJoin').on('click', function () {
    var roomNo = $('#txtRoomNo').val();
    var nick = $('#txtNickName').val();
    if (!roomNo) {
        alert("请输入RoomNo");
        return;
    }
    var msg = {
        action: 'join',
        msg: roomNo,
        nick: nick
    };
    if (CheckWebSocketConnected(webSocket)) {
        SendMessage(msg)
    }
})

$('#btnSend').on('click', function () {
    var message = $('#txtMsg').val();
    var nick = $('#txtNickName').val();
    if (!message) {
        alert("请输入发生的内容");
        return;
    }
    if (CheckWebSocketConnected(webSocket)) {
        SendMessage(
            {
                action: 'send_to_room',
                msg: message,
                nick: nick
            }
        )
    }
})

$('#btnLeave').on('click', function () {
    var nick = $('#txtNickName').val();
    var msg = {
        action: 'leave',
        roomNo: '',
        nick: nick
    };
    if (CheckWebSocketConnected(webSocket)) {
        SendMessage(JSON.stringify(msg));
    }
});

$("#btnDisConnect").on("click", function () {
    if (CheckWebSocketConnected(webSocket)) {
        //部分浏览器调用close()方法关闭WebSocket时不支持传参
        //webSocket.close(001, "closeReason");
        webSocket.close();
    }
});

var CheckWebSocketConnected=function (websocket) {
    return webSocket.readyState == 1
}

var SendMessage=function (data) {
    webSocket.send(JSON.stringify(data));
}
