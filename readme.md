#RabbitMQ Event Bus

Lightweight event bus with RabbitMQ as transportation mechanism.

## Events


```
+------------+               +------------+      +------------+
|            |               |            |		 |            |
|   sender   |               | receiver<T>|		 | receiver<T>|
|            |               |            |		 |            |
+------------+               +------------+		 +------------+
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
