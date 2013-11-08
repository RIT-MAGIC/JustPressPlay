/*
var $cache = $('.stickyPanel');
var navHeight = $('.top-bar').height() + 20;
var distance = $cache.offset().top;
var scrollThreshold = distance - navHeight;

function fixDiv() {
    



    if ($cache.length > 0) {
        //var distance = $cache.length ? $cache.offset().top : 0,
        //$window = $(window);
        
        //alert(navHeight + "   " + distance + "  " + scrollThreshold);
        //alert(distance);
        //var offst = this.outerHeight($('.top-bar'));
       // var offst = 0;

        if ($(window).scrollTop() >= scrollThreshold ) {
            $cache.css({ 'position': 'fixed', 'top': navHeight });
            //$('body').css('padding-top', offst);
        }
        else {
            $cache.css({ 'position': 'relative', 'top': 'auto' });
            //alert('Shiiiiiiiiiit');
            //$('body').css('padding-top', '0');
        }
    }
}
$(window).scroll(fixDiv);
fixDiv();
*/




// QR GENERATION
var create_qrcode = function (text, typeNumber, errorCorrectLevel, table) {

    var qr = qrcode(typeNumber || 4, errorCorrectLevel || 'M');
    qr.addData(text);
    qr.make();

    //	return qr.createTableTag();
    return qr.createImgTag();
};


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


    
});

