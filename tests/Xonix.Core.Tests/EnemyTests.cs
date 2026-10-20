namespace Xonix.Core.Tests;

public class EnemyTests
{
    [Fact]
    public void Constructor_StraightHeading_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Enemy(new Cell(5, 5), 0, 1));

    [Fact]
    public void Step_OpenSpace_MovesDiagonally()
    {
        var enemy = new Enemy(new Cell(5, 5), 1, -1);

        enemy.Step(_ => true);

        Assert.Equal(new Cell(6, 4), enemy.Position);
    }

    [Fact]
    public void Step_SideWall_BouncesHorizontally()
    {
        var enemy = new Enemy(new Cell(4, 10), 1, 1);

        enemy.Step(cell => cell.X < 5);

        Assert.Equal(new Cell(3, 11), enemy.Position);
        Assert.Equal((-1, 1), (enemy.Dx, enemy.Dy));
    }

    [Fact]
    public void Step_Corner_ReversesBothHeadings()
    {
        var enemy = new Enemy(new Cell(4, 4), 1, 1);

        enemy.Step(cell => cell.X < 5 && cell.Y < 5);

        Assert.Equal(new Cell(3, 3), enemy.Position);
    }

    [Fact]
    public void Step_OnlyDiagonalClosed_ReversesBothHeadings()
    {
        var enemy = new Enemy(new Cell(5, 5), 1, 1);

        enemy.Step(cell => cell != new Cell(6, 6));

        Assert.Equal(new Cell(4, 4), enemy.Position);
    }

    [Fact]
    public void Step_BoxedIn_StaysPutInsteadOfCuttingCorners()
    {
        Cell[] walls = [new(6, 5), new(5, 6), new(4, 5), new(5, 4)];
        var enemy = new Enemy(new Cell(5, 5), 1, 1);

        enemy.Step(cell => !walls.Contains(cell));

        Assert.Equal(new Cell(5, 5), enemy.Position);
    }
}
