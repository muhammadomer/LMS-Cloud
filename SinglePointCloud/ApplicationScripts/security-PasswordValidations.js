validateUserPassword = function (password, userName) {
    //console.log('User Name: ' + userName);
    //console.log('Password: ' + password);
    //console.log(password.toLowerCase().indexOf(userName.toLowerCase()));
    var validationExpression = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%*?&])[A-Za-z\d$@$!%*?&]{8,}/;
    //if (password.toLowerCase().indexOf(userName.toLowerCase()) >= 0) {
    //    return "Password cannot contain username";
    //}
    //else 
    if (!validationExpression.test(password)) {
        return "Passwords must have at least 8 characters with one in uppercase, one lowercase, one number and one special character.";
    }
    else {
        return "true";
    }

}

validateUserPasswordWithConfirmPassword = function (password, confirmPassword, userName) {
    var validationExpression = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@$!%£*?&])[A-Za-z\d$@$!%£*?&]{8,}/;
    if (password != confirmPassword) {
        return "The password you supplied do not match";
    }
    //if (password.toLowerCase().indexOf(userName) >= 0) {
    //    return "Password cannot contain username";
    //}
    else if (!validationExpression.test(password)) {
        return "Passwords must have at least 8 characters with one in uppercase, one lowercase, one number and one special character.";
    }
    else {
        return "true";
    }
}