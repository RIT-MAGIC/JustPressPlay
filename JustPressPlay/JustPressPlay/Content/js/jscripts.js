/* 
 * jscripts.js
 * Just Press Play v3
 */

$(document).ready(function () {

    /***************************
        SITE-WIDE FEATURES
    ***************************/

    FastClick.attach(document.body);


    // NAVIGATION DROPDOWNS

    var activeDropdownId = "";

    $('body').click(function (e) {

        if ($(e.target).closest('.dropdownContainer').length > 0)
        {   // Click on dropdown
            //e.stopPropigation();
            return;
        }


        if ($(e.target).closest('[data-dropdownid]').length > 0)
        {   // Click on dropdown button
            var parentButton = $(e.target).closest('[data-dropdownid]');
            var targetId = parentButton.data('dropdownid');

            e.preventDefault();

            $('#' + targetId).toggleClass('active');

            if (targetId == activeDropdownId)
                activeDropdownId = "";
            else
            {
                $('#' + activeDropdownId).toggleClass('active', false);
                activeDropdownId = targetId;
            }


            return;
        }
           
        $('#' + activeDropdownId).toggleClass('active', false);
        activeDropdownId = "";

    });

    // END NAVIGATION DROPDOWNS



    // SCROLL BOXES
    
    $('.scroll').slimScroll();

    $('.dropdownScroll').slimScroll({
        height: '250px',
        size: '6px',
        position: 'right',
        color: '#000',
        alwaysVisible: false,
        distance: '1px',
        railVisible: false,
        railDraggable: true,
        railColor: '#222',
        railOpacity: 0.3,
        wheelStep: 10,
        allowPageScroll: false,
        disableFadeOut: false
    });


    // Bind click event to friend request buttons (use a generic class selector)
    // On click call ajax.submit (check function name)
    // 

    

    
});

