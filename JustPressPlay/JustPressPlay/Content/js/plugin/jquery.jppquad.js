(function ($) {

    $.fn.jppquad = function (options) {

        var self = this;

        // Create settings object
        var settings = $.extend({
            // Defaults
            createPoints: 0,
            learnPoints: 0,
            explorePoints: 0,
            socializePoints: 0,
            maxThreshold: 100,
            thresholds: {
                0: 4,
                1: 25,
                2: 50
            }
        }, options);

        var findPercent = function( threshold ) {
            return (threshold <= settings.maxThreshold) ? Math.round((threshold / settings.maxThreshold)*50) : 50;
        }
        var styleThreshold = function (threshold) {

            var styling = 'width: ' + findPercent( threshold ) * 2 + '%; '
                        + 'height: ' + findPercent( threshold ) * 2 + '%; '
                        + 'top: ' + ( 50 - findPercent( threshold ) ) + '%; '
                        + 'left: ' + (50 - findPercent(threshold)) + '%;';

            return styling;
        }
        var updateQuads = function () {
            self.find('.createQuad').css({ width: findPercent(settings.createPoints) + '%', height: findPercent(settings.createPoints) + '%' });
            self.find('.learnQuad').css({ width: findPercent(settings.learnPoints) + '%', height: findPercent(settings.learnPoints) + '%' });
            self.find('.exploreQuad').css({ width: findPercent(settings.explorePoints) + '%', height: findPercent(settings.explorePoints) + '%' });
            self.find('.socialQuad').css({ width: findPercent(settings.socializePoints) + '%', height: findPercent(settings.socializePoints) + '%' });
        }



        // Ensure proper styling
        this.addClass('jppquad');



        // Build HTML
        // This prevents the view from collapsing upon itself
        this.append('<div class="heightFill"></div>');



        // Draw Background
        this.append('<div class="quadBackground"></div>');



        // Create Quads
        this.append(
            '<div class="circleContainer">' +
                '<div class="createQuad transition" style="width: 0; height: 0"></div>' +
                '<div class="learnQuad transition" style="width: 0; height: 0"></div>' +
                '<div class="exploreQuad transition" style="width: 0; height: 0"></div>' +
                '<div class="socialQuad transition" style="width: 0; height: 0"></div>' +
            '</div>');
        


        // Draw Thresholds
        // Loop through settings.thresholds
        $.each(settings.thresholds, function (index, value) {
            if( value < settings.maxThreshold && value > 0 )
                self.find('.circleContainer').append('<div class="threshold" style="' + styleThreshold( value ) + '"><p>' + value + '</p></div>');
        });
        self.find('.circleContainer').append('<div class="threshold max" style="' + styleThreshold( settings.maxThreshold ) + '"><p>' + settings.maxThreshold + '</p></div>');



        // Draw Counts
        var displayForLarge = (self.width() > 150) ? 'large' : '';
        this.append('<div class="createLabel ' + displayForLarge + '"><p>' + settings.createPoints + '</p></div>');
        this.append('<div class="learnLabel ' + displayForLarge + '"><p>' + settings.learnPoints + '</p></div>');
        this.append('<div class="exploreLabel ' + displayForLarge + '"><p>' + settings.explorePoints + '</p></div>');
        this.append('<div class="socialLabel ' + displayForLarge + '"><p>' + settings.socializePoints + '</p></div>');



        // Delay animation
        window.setTimeout(updateQuads, 400);


        return this;

    };

}(jQuery));
