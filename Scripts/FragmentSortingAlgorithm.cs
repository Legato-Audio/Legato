
namespace Legato
{
    public enum SortingAlgorithms { CircleQueue, Random, Shuffle }
    public abstract class FragmentSortingAlgorithm
    {
        protected FragmentSortingAlgorithm() { }
        public abstract void Start(Fragment[] fragments);
        public abstract Fragment GetNextFragment();
    }
}
