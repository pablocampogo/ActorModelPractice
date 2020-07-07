using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyActorModel
{
    public class PrintMyActorRefActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case "printit":
                    IActorRef secondRef = Context.ActorOf(Props.Empty, "second-actor");
                    Console.WriteLine($"Second: {secondRef}");
                    break;
            }
        }
    }

    public class StartStopActor1 : UntypedActor
    {
        protected override void PreStart()
        {
            Console.WriteLine("first started");
            Context.ActorOf(Props.Create<StartStopActor2>(), "second");
        }

        protected override void PostStop() => Console.WriteLine("first stopped");

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case "stop":
                    Context.Stop(Self);
                    break;
            }
        }
    }

    public class StartStopActor2 : UntypedActor
    {
        protected override void PreStart() => Console.WriteLine("second started");
        protected override void PostStop() => Console.WriteLine("second stopped");

        protected override void OnReceive(object message)
        {
        }
    }

    public class SupervisingActor : UntypedActor
    {
        private IActorRef child = Context.ActorOf(Props.Create<SupervisedActor>(), "supervised-actor");

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case "failChild":
                    child.Tell("fail");
                    break;
            }
        }
    }

    public class SupervisedActor : UntypedActor
    {
        protected override void PreStart() => Console.WriteLine("supervised actor started");
        protected override void PostStop() => Console.WriteLine("supervised actor stopped");

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case "fail":
                    Console.WriteLine("supervised actor fails now");
                    throw new Exception("I failed!");
            }
        }
    }

    public class IotSupervisor : UntypedActor
    {
        public ILoggingAdapter Log { get; } = Context.GetLogger();

        protected override void PreStart() => Log.Info("IoT Application started");
        protected override void PostStop() => Log.Info("IoT Application stopped");

        // No need to handle any messages
        protected override void OnReceive(object message)
        {
        }

        public static Props Props() => Akka.Actor.Props.Create<IotSupervisor>();
    }

    public class IotApp
    {
        public static void Init()
        {
            using (var system = ActorSystem.Create("iot-system"))
            {
                // Create top level supervisor
                var supervisor = system.ActorOf(IotSupervisor.Props(), "iot-supervisor");
                // Exit the system after ENTER is pressed
                Console.ReadLine();
            }
        }
    }

    class Program 
    {
        static void Main(string[] args)
        {
            var sys = ActorSystem.Create("IDGActorSystem");

            Console.WriteLine("PATH OF ACTORS");
            var firstRef = sys.ActorOf(Props.Create<PrintMyActorRefActor>(), "first-actor");
            Console.WriteLine($"First: {firstRef}");
            firstRef.Tell("printit", ActorRefs.NoSender);
            Console.ReadLine();

            Console.WriteLine("HIERACHY AND LIFECYCLE");
            var first = sys.ActorOf(Props.Create<StartStopActor1>(), "first");
            first.Tell("stop");
            Console.ReadLine();

            Console.WriteLine("HIERACHY AND FAILURE HANDLING");
            var supervisingActor = sys.ActorOf(Props.Create<SupervisingActor>(), "supervising-actor");
            supervisingActor.Tell("failChild");
            Console.ReadLine();

            Console.WriteLine("SOMETHING MORE REAL");
            IotApp.Init();
            Console.ReadLine();
        }
    }
}
