window.SystemDrawingInterop = {
    SetupCanvas: function (id)
    {
        var canvas = document.getElementById(id);
        if (canvas)
        {
            // Get the device pixel ratio, falling back to 1.
            var dpr = window.devicePixelRatio || 1;
            // Get the size of the canvas in CSS pixels.
            var rect = canvas.getBoundingClientRect();
            // Give the canvas pixel dimensions of their CSS
            // size * the device pixel ratio.
            canvas.width = rect.width * dpr;
            canvas.height = rect.height * dpr;
            var ctx = canvas.getContext('2d');
            // Scale all drawing operations by the dpr, so you
            // don't have to worry about the difference.
            ctx.scale(dpr, dpr);
            return dpr;
        }

        return 1;
    },
    PointIsInPath : function (path, x, y)
    {
        var canvas = document.getElementsByTagName("canvas")[0];
        if (canvas)
        {
            var path2d = new Path2D(path);
            return canvas.isPointInPath(path, x, y);
        }
        
        return false;
    }
};