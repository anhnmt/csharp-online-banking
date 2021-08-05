// This optional function html-encodes messages for display in the page.
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}

$(function () {
    // Reference the auto-generated proxy for the hub.
    var connection = $.connection;
    var hub = connection.chatHub;
    var user = '';

    // Create a function that the hub can call back to display messages.
    hub.client.addNewMessageToPage = function (name, message) {
        var msg = `
                        <div class="direct-chat-msg ${user == name ? 'right' : ''} ">
                            <div class="direct-chat-infos clearfix">
                                <span class="direct-chat-name ${user == name ? 'float-right' : 'float-left'}">${htmlEncode(name)}</span>
                                <span class="direct-chat-timestamp ${user == name ? 'float-left' : 'float-right'}">23 Jan 2:05 pm</span>
                            </div>
                            <!-- /.direct-chat-infos -->
                            <img class="direct-chat-img" src="https://adminlte.io/themes/dev/AdminLTE/dist/img/user3-128x128.jpg" alt="message user image">
                            <!-- /.direct-chat-img -->
                            <div class="direct-chat-text">
                                ${htmlEncode(message)}
                            </div>
                            <!-- /.direct-chat-text -->
                        </div>`;
        // Add the message to the page.
        $('#discussion')
            .append(msg)
            .animate({ scrollTop: $('#discussion').prop("scrollHeight") }, 150);
    };
    // Set initial focus to message input box.
    $('#message').focus();
    // Start the connection.
    connection.hub.start().done(function () {
        var connectionId = connection.hub.id;

        $('#chat').submit(function () {
            if ($('#message').val() !== "") {
            hub.server.send($('#message').val());
            // Clear text box and reset focus for next comment.
            $('#message').val('').focus();
            }
            event.preventDefault();
        });
    });

    connection.hub.disconnected(function () {
        setTimeout(function () {
            connection.hub.start();
        }, 3000); // Restart connection
    });

});