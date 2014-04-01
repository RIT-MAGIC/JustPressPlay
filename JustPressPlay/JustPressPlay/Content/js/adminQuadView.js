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
    $('#PointsCreate').val(points.createPoints);
    $('#PointsLearn').val(points.learnPoints);
    $('#PointsExplore').val(points.explorePoints);
    $('#PointsSocialize').val(points.socialPoints);

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
   

   
    $("#PointsCreate").change(function () {

        $('#createQuad').removeClass('quad' + createPoints);
        createPoints = $('#PointsCreate').val();
        $('#createQuad').addClass('quad' + createPoints);
    })
    .change();
    $("#PointsLearn").change(function () {

        $('#learnQuad').removeClass('quad' + learnPoints);
        learnPoints = $('#PointsLearn').val();
        $('#learnQuad').addClass('quad' + learnPoints);
    })
    .change();
    $("#PointsExplore").change(function () {

        $('#exploreQuad').removeClass('quad' + explorePoints);
        explorePoints = $('#PointsExplore').val();
        $('#exploreQuad').addClass('quad' + explorePoints);
    })
    .change();
    $("#PointsSocialize").change(function () {

        $('#socialQuad').removeClass('quad' + socialPoints);
        socialPoints = $('#PointsSocialize').val();
        $('#socialQuad').addClass('quad' + socialPoints);
    })
    .change();
});