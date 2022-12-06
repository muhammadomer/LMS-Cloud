AjaxPostRequestWithRequestPerameters = function (url, requestPerameters, callbcak) {
    //console.log(url);
    //console.dir(requestPerameters);
    var token = $('[name=__RequestVerificationToken]').val();
    var dataToReturn = null;
    try {
        $.ajax({
            type: "POST",
            url: url,
            headers: { '__RequestVerificationToken': token },
            data: JSON.stringify(requestPerameters),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                if (data != null) {
                    //if (data.d != '' && data.d != null && data.d != "null") {
                        dataToReturn = data;
                        /*if (data.d.length > 0) {
                            //console.dir(data.d);
                            dataToReturn = data.d;
                        }*/
                    //}
                    //console.log('Returning Data');
                    //console.dir(dataToReturn);
                }
                callbcak(dataToReturn);
            },
            error: function (xhr, textStatus, errorThrown) {
                if (isUserAborted(xhr)) {
                    return;
                }

                if (xhr.getResponseHeader("X-Responded-JSON") != null
                    && JSON.parse(xhr.getResponseHeader("X-Responded-JSON")).status == "401") {
                    console.log("Error 401 found."); console.log("Error URL: " + this.url);
                    window.location.href = "../Account/WarningPage";
                    return;
                }


                console.log('Status: ' + textStatus); console.log("URL:" + this.url);
                console.log('Error: ' + errorThrown);
                window.location.href = "../Account/WarningPage";
                callbcak(dataToReturn);
            }
        });
    }
    catch (e) {
        console.log('Error In AjaxPostRequestWithRequestPerameters: ' + e.message);
        $().toastmessage("showErrorToast", "Sorry, an unexpected error occurred.");
        return null;
    }
}

AjaxPostRequestWithoutRequestPerameters = function (url, callbcak) {
    var token = $('[name=__RequestVerificationToken]').val();
    //console.log(url);
    //console.dir(requestPerameters);
    var dataToReturn = null;
    try {
        $.ajax({
            type: "POST",
            url: url,
            headers: { '__RequestVerificationToken': token },
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
                if (data != null) {
                    //if (data.d != '' && data.d != null && data.d != "null") {
                    dataToReturn = data;
                    /*if (data.d.length > 0) {
                        //console.dir(data.d);
                        dataToReturn = data.d;
                    }*/
                    //}
                    //console.log('Returning Data');
                    //console.dir(dataToReturn);
                }
                //console.log('Returning Data AjaxPostRequestWithoutRequestPerameters');
                //console.dir(dataToReturn);
                callbcak(dataToReturn);
            },
            error: function (xhr, textStatus, errorThrown) {
                if (isUserAborted(xhr)) {
                    return;
                }

                if (xhr.getResponseHeader("X-Responded-JSON") != null
                    && JSON.parse(xhr.getResponseHeader("X-Responded-JSON")).status == "401") {
                    console.log("Error 401 found."); console.log("Error URL: " + this.url);
                    window.location.href = "../Account/WarningPage";
                    return;
                }


                console.log('Status: ' + textStatus); console.log("URL:" + this.url);
                console.log('Error: ' + errorThrown);
                window.location.href = "../Account/WarningPage";
                callbcak(dataToReturn);
            }
        });
    }
    catch (e) {
        console.log('Error In AjaxPostRequestWithoutRequestPerameters: ' + e.message);
        $().toastmessage("showErrorToast", "Sorry, an unexpected error occurred.");
        return null;
    }
}

function isUserAborted(xhr) {
    return !xhr.getAllResponseHeaders();
}



function setHeartbeat() {
    setInterval("heartbeat()", 5 * 60 * 1000); // every 5 min
}

function heartbeat() {
    $.get(
        "/SessionHeartbeat.ashx",
        null,
        function (data) {
            setHeartbeat();
        },
        "json"
    );
}