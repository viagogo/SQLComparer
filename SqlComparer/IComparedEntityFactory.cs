using SqlComparer.Model;

namespace SqlComparer
{
    public interface IComparedEntityFactory
    {
        ComparedEntity Create(string representation);
    }
}
