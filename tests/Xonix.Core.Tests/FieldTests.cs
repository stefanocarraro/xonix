namespace Xonix.Core.Tests;

public class FieldTests
{
    [Fact]
    public void New_FrameIsLandAndInteriorIsSea()
    {
        var field = new Field();

        Assert.Equal(Tile.Land, field[new Cell(0, 0)]);
        Assert.Equal(Tile.Land, field[new Cell(Field.Border - 1, 20)]);
        Assert.Equal(Tile.Sea, field[new Cell(Field.Border, Field.Border)]);
        Assert.Equal(Tile.Land, field[new Cell(Field.Width - 1, Field.Height - 1)]);
        Assert.Equal(0, field.FilledPercent);
    }

    [Fact]
    public void Indexer_OutsideField_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Field()[new Cell(Field.Width, 0)]);

    [Fact]
    public void Capture_ClaimsTrailAndBallFreeRegion()
    {
        var field = WithWall(x: 10);

        int captured = field.Capture([new Cell(40, 20)]);

        int wallCells = Field.Height - 2 * Field.Border;
        Assert.Equal(wallCells * 9, captured);
        Assert.Equal(Tile.Land, field[new Cell(10, 20)]);
        Assert.Equal(Tile.Land, field[new Cell(5, 20)]);
        Assert.Equal(Tile.Sea, field[new Cell(40, 20)]);
    }

    [Fact]
    public void Capture_KeepsEveryRegionHoldingABall()
    {
        var field = WithWall(x: 10);

        int captured = field.Capture([new Cell(40, 20), new Cell(5, 5)]);

        Assert.Equal(Field.Height - 2 * Field.Border, captured);
        Assert.Equal(Tile.Sea, field[new Cell(5, 20)]);
    }

    [Fact]
    public void Capture_DiagonalGapDoesNotConnectRegions()
    {
        var field = new Field();
        // A diagonal wall: its cells touch only at corners, and so do the sea cells on either side.
        for (int i = Field.Border; i < Field.Height - Field.Border; i++)
        {
            field[new Cell(i, i)] = Tile.Trail;
        }

        field.Capture([new Cell(Field.Width - 4, Field.Border)]);

        Assert.Equal(Tile.Land, field[new Cell(Field.Border, Field.Height - Field.Border - 1)]);
    }

    [Fact]
    public void FilledPercent_CountsClaimedInterior()
    {
        var field = new Field();
        // 33 of the 44 interior rows are exactly 75% of the interior.
        for (int y = Field.Border; y < Field.Border + 33; y++)
        {
            for (int x = Field.Border; x < Field.Width - Field.Border; x++)
            {
                field[new Cell(x, y)] = Tile.Land;
            }
        }

        Assert.Equal(75, field.FilledPercent);
    }

    private static Field WithWall(int x)
    {
        var field = new Field();
        for (int y = Field.Border; y < Field.Height - Field.Border; y++)
        {
            field[new Cell(x, y)] = Tile.Trail;
        }

        return field;
    }
}
