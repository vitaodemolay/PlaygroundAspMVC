/// <reference path="../inputmask/inputmask.js" />
/// <reference path="../inputmask/jquery.inputmask.js" />
/// <reference path="../inputmask/inputmask.extensions.js" />
/// <reference path="../inputmask/inputmask.date.extensions.js" />
/// <reference path="../inputmask/inputmask.numeric.extensions.js" />
/// <reference path="../inputmask/inputmask.phone.extensions.js" />
/// <reference path="../inputmask/inputmask.regex.extensions.js" />

$(document).ready(function () {
    $(".phone").inputmask("mask", { "mask": "(99)9999[9]-9999" }, { reverse: true });
    $(".celphone").inputmask("mask", { "mask": "(99)99999-9999" }, { reverse: true });
    $(".cpf").inputmask("mask", { "mask": "999.999.999-99" }, { reverse: true });
    $(".cnpj").inputmask("mask", { "mask": "99.999.999/9999-99" }, { reverse: true });
    $(".cep").inputmask("mask", { "mask": "99999-999" }, { reverse: true });
    $('.agenciabanco').inputmask({ "mask": "9{3}-9", "greedy": true }, { reverse: true });
    $('.contabanco').inputmask({ "mask": "9{4,6}-9", "greedy": true }, { reverse: true });
    $(".dateMask").inputmask("mask", { "mask": "99/99/9999" }, { reverse: true });
    $('.tokenvalid').inputmask({ "mask": "[9|a]{4}-[9|a]{3}-[9|a]{3}", "greedy": true }, { reverse: true });
});