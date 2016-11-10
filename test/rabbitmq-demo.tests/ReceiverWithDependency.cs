namespace rabbitmq_demo.tests
{
    class ReceiverWithDependency : IReceive<int>
    {
        private readonly IDependency _dependency;

        public ReceiverWithDependency(IDependency dependency)
        {
            _dependency = dependency;
        }
        public void Execute(int item)
        {
            _dependency.Foo();
        }
    }
}
