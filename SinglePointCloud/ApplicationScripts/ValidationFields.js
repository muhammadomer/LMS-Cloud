
var specialCharacterError = "Special characters are not allowed.";

function htmlEncode(value) {
    return $('<div/>').text(value).html();
}


function htmlDecode(value) {
    return $('<div/>').html(value).text();
}

function htmlTagsValidation(string) {
    
    var regex = /(<([^>]+)>)/ig;

    var exist = regex.test(string);

    return exist;
}

function validateEmail(email) {
    var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(String(email).toLowerCase());
}