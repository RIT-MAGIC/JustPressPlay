var totalPoints = 4;
var pointCount = 0;
var createPoints = 0;
var learnPoints = 0;
var explorePoints = 0;
var socialPoints = 0;

function updateSelectors() {
    pointCount = createPoints + learnPoints + explorePoints + socialPoints;

    if (pointCount >= 4)
    {
        //Disable if option
    }
}

function setupQuad(points) {

    //Set initial values
    $('#createSelect').val(points.createPoints);
    $('#learnSelect').val(points.learnPoints);
    $('#exploreSelect').val(points.explorePoints);
    $('#socialSelect').val(points.socialPoints);

    $('#createQuad').addClass('quad' + points.createPoints);
    $('#learnQuad').addClass('quad' + points.learnPoints);
    $('#exploreQuad').addClass('quad' + points.explorePoints);
    $('#socialQuad').addClass('quad' + points.socialPoints);
}



$(document).ready(function () {

    $('#thresholdSelect').select2();
    $('#qrAdmins').select2();
    $('#userContents').select2();
    $('#systemTriggers').select2();

    //var id = $("input[@name=testGroup]:checked").attr('id');
    //check
    $("input[name='avaDate']").change(function () {
        if ($('#dateRestricted').is(':checked'))
            $('#dateChoice').toggleClass('active', true);
        else
            $('#dateChoice').removeClass('active');
    });

    $("input[name='achievementType']").change(function () {
        if ($('#z2').is(':checked'))
            $('#thresholdOptions').toggleClass('active', true);
        else
            $('#thresholdOptions').removeClass('active');
    });

    $("input[name='validationType']").change(function () {

        $('#qrType').toggleClass('active', false);
        $('#userContentType').toggleClass('active', false);
        $('#systemTriggerType').toggleClass('active', false);

        if ($('#qrChoice').is(':checked')) {
            $('#qrType').toggleClass('active', true);
        }
        else if ($('#userContentChoice').is(':checked')) {
            $('#userContentType').toggleClass('active', true);
        }
        else if ($('#systemTriggerChoice').is(':checked')) {
            $('#systemTriggerType').toggleClass('active', true);
        }
    });
   

   
    $("#createSelect").change(function () {

        $('#createQuad').removeClass('quad' + createPoints);
        createPoints = $('#createSelect').val();
        $('#createQuad').addClass('quad' + createPoints);
    })
    .change();
    $("#learnSelect").change(function () {

        $('#learnQuad').removeClass('quad' + learnPoints);
        learnPoints = $('#learnSelect').val();
        $('#learnQuad').addClass('quad' + learnPoints);
    })
    .change();
    $("#exploreSelect").change(function () {

        $('#exploreQuad').removeClass('quad' + explorePoints);
        explorePoints = $('#exploreSelect').val();
        $('#exploreQuad').addClass('quad' + explorePoints);
    })
    .change();
    $("#socialSelect").change(function () {

        $('#socialQuad').removeClass('quad' + socialPoints);
        socialPoints = $('#socialSelect').val();
        $('#socialQuad').addClass('quad' + socialPoints);
    })
    .change();
});