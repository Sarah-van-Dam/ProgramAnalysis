namespace Analyses
{
    public interface IConstraints
    {
        /// <summary>
        /// True if current object is subset of <paramref name="other" />:
        ///     are the keys the same  - are the tuples in this also contained in other
        ///     for all keys - are the tuples in this also contained in other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool IsSubset(IConstraints other);
        bool IsSuperSet(IConstraints other);

        IConstraints Clone();

        void Join(IConstraints other);

        string ToString();

    }
}