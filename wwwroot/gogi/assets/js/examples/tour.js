'use strict';
$(document).ready(function () {

    $(document).on('click', 'a.tour', function () {
        var enjoyhint_instance = new EnjoyHint({});

        enjoyhint_instance.set([
            {
                'next .header': 'Quick toolbar.',
            },
            {
                'next .navigation': 'Navigation to navigate the page.',
            },
            {
                'next .tourbc': 'Custom navigation',
            }
        ]);
        enjoyhint_instance.run();
    });

});