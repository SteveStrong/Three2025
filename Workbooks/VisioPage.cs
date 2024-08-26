
using Blazor.Extensions.Canvas.Canvas2D;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;

namespace Three2025.Model;

public class VisioPage : FoPage2D
{
    public VisioPage(string name, string color) : base(name,color)
    {

    }

    public VisioPage(string name, int width, int height, string color)
        : base(name, width, height, color)
    {
    }


    public override async Task<bool> RenderDetailed(Canvas2DContext ctx, int tick, bool deep = true)
    {
        if (!IsVisible) return false;

        await ctx.SaveAsync();

        await UpdateContext(ctx, tick);


        var margin = PageMargin.AsPixels();
        var width = PageWidth.AsPixels() + (2.0 * margin);
        var height = PageHeight.AsPixels() + (2.0 * margin);

        Width = (int)width;
        Height = (int)height;

        await ctx.SetFillStyleAsync("White");
        await ctx.FillRectAsync(0, 0, width, height);


        await DrawPageName(ctx);

        await ctx.SetFillStyleAsync(Color);
        await ctx.SetGlobalAlphaAsync(1.0F);
        await ctx.FillRectAsync(margin, margin, PageWidth.AsPixels(), PageHeight.AsPixels());

        await RenderGrid(ctx);

        Shapes2D.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));

        // this draws connectors over top of the 2D shapes
        Shapes1D.ForEach(async child => await child.RenderDetailed(ctx, tick, deep));


        await ctx.RestoreAsync();
        return true;
    }

 
}