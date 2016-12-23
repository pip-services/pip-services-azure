After regeneration of help documentation replace script in Index.html with this one:

        window.location.replace"html/html/2c590b49-022b-4271-81bf-e811e45e45f3.htm");

with this one:

        var base = window.location.href;
        base = base.substr(0, base.lastIndexOf("/") + 1);
        window.location.replace(base + "html/2c590b49-022b-4271-81bf-e811e45e45f3.htm");
