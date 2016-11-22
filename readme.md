#RabbitMQ Event Bus

Lightweight event bus with RabbitMQ as transportation mechanism.

## Events


```
                              +------------+      +------------+
+------------+               +------------+|     +------------+|
|            |               |            ||	 |            ||
|   sender   |               | receiver<T>||	 | receiver<T>||
|            |               |            |+	 |            |+
+------------+               +------------+ 	 +------------+
      ||                           ||			       ||
PublishEvent<T>             SubscribeEvents<T>	SubscribeEvents<T>
      ||                           ||			       ||
	  ||                           \/			       \/
	  \/                     +------------+		 +------------+
-----------------------------|            |------|            |
    RabbiqMQ Exchange        |  listener  |      |  listener  |
-----------------------------|            |------|            |
							 +------------+		 +------------+
```
For **sending** events inject the ISender interface as a dependency into the class that is sending.
For **receiving** events implement the IReceive interface and **subscribe** to an instance of the listener class.


## Commands

```
+------------+               +------------+
|            |               |            |
|   sender   |               | receiver<T>|
|            |               |            |
+------------+               +------------+
      ||                           ||
PublishCommand<T>           SubscribeCommands<T>
      ||                           ||
	  ||                           \/
	  \/                     +------------+
-----------------------------|            |
    RabbiqMQ Queue           |  listener  |
-----------------------------|            |
							 +------------+
```
For **sending** commands inject the ISender interface as a dependency into the class that is sending.
For **receiving** events commands the IReceive interface and **subscribe** to an instance of the listener class.

## Testing
Unit test a sending class by injecting a substitute for the ISender argument.
```cs
[Fact]
public void AddPersonPublishesPersonCreatedCommand()
{
    var sender = Substitute.For<ISender>();
    var controller = new PeopleController(sender);

    var command = new CreatePerson { FirstName = "first", LastName = "last" };
    controller.Create(command);

    sender.Received(1).PublishCommand(command);
}
```

Unit test a receiving class by invoking the Execute method.
```cs
[Fact]
public void FrontEndServiceStoresPersonCreatedEvents()
{
    // Arrange
    var dbset = Substitute.For<DbSet<Person>>();
    var context = Substitute.For<IFrontEndContext>();
    context.People.Returns(dbset);

    var service = new FrontEndService(context);

    // Act
    service.Execute(new PersonCreated { });
    
    // Assert
    dbset.Received(1).Add(Arg.Any<Person>());
    context.Received(1).SaveChanges();
}
```

For integration testing you use the TestSender and TestListener classes in conjuction
with the BlockingReceiver<T> class.
```cs
[Fact]
public void AddPersonPublishesPersonCreatedEvent()
{
    using (var service = new BlockingReceiver<CreatePerson>())
    using (var listener = new TestListener())
    using (var sender = listener.Sender())
    {
        service.SubscribeToEvents(listener);

        var controller = new PeopleController(sender);
        controller.Create(new CreatePerson { FirstName = "first", LastName = "last" });

        var command = service.Next();
        Assert.NotNull(command);
        Assert.Equal("first", command.FirstName);
        Assert.Equal("last", command.LastName);
    }
}
```

You can use a TestSender and obtain a listener from that, or the other way around if that makes more sense for your scenario.
Point is every self instantiated TestSender or TestListener are created in seperate namespace so two
distinct senders or listeners will not pick up messages from one another.

## Subscribing to Events and Commands
The BlockingReceiver is easy to subscribe because of the helper method available on the instance. To 
subscribe your own receivers you'll have to provide an Autofac container that manages the lifetime
of your objects and dependencies. You can use the RegisterReceiverFor extension method for that:

```cs
var builder = new ContainerBuilder();
builder
	.Register(c => Substitute.For<IDependency>());

builder
	.RegisterReceiverFor<ReceiverWithDependency, PersonCreated>();

using (var container = builder.Build())
using (var listener = new Listener(...))
{
	listener.SubscribeEvents<PersonCreated>(container);
}
```

Or you can do it yourself:
```cs
var builder = new ContainerBuilder();
builder
	.Register(c => Substitute.For<IDependency>());

builder
	.RegisterType<ReceiverWithDependency>()
	.As<IReceive<PersonCreated>>();

using (var container = builder.Build())
using (var listener = new Listener(...))
{
	listener.SubscribeEvents<PersonCreated>(container);
}
```

You have to specify the message type when subscribing to events and commands. 
The scope of the container has to be wider than the scope of the listener because otherwise the container may already be disposed while the listener is still active.