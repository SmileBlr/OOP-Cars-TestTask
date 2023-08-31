using System;
using System.Collections.Generic;
using System.Linq;

namespace TestCars
{
    /*
     * Pattern facade in the implementation of the car is used for simplified access to the subsystems of the car.
     * Pattern observer monitors the actions of the car and calculates the damage of its parts, this removes the load from the facade 
     * and can allow more flexible damage calculation.
     * There are two levels of abstraction in car parts, the highest CarParts and inherited engine/tires/suspension.
     * The body type is inherent in all cars and is set at a higher level of abstraction in the constructor of the Car class,
     * at a lower level in the automaker classes, the car model is set in the constructor.
    */
    class Program
    {
        static void Main(string[] args)
        {
            Ford ford = new Ford("Ford Fusion", BodyType.Sedan);
            Fiat fiat = new Fiat("Fiat Stilo 2.4", BodyType.Hatchback);
            CarCatalog carCatalog = new CarCatalog(new List<Car> { ford, fiat });

            var carsToBuy = carCatalog.ChooseCarsByBodyType(BodyType.Hatchback);
            var myCar = carsToBuy[0];
            myCar.SetNewOwner("Dimon");

            myCar.CarFacade.Gas();
            myCar.CarFacade.Break();

            var partsForRepair = myCar.CarFacade.Diagnostic();
            myCar.CarFacade.Repair(partsForRepair);
        }
    }
    //Provides a list of cars available for sale, with the ability to sort by body type
    public class CarCatalog
    {
        public List<Car> CarsForSale { get; private set; }

        public CarCatalog(List<Car> cars)
        {
            CarsForSale = cars;
        }

        public void BuyCar(Car car, string newOwnerName)
        {
            (car as ICarOwner).SetNewOwner(newOwnerName);
        }
        public Car[] ChooseCarsByBodyType(BodyType body)
        {
            return CarsForSale.Where(car => car.Body == body).ToArray();
        }
    }

    //basic implementation of car functionality
    public abstract class Car : ICarOwner
    {
        public string OwnerName { get; protected set; }
        public string CarModel { get; protected set; }
        public BodyType Body { get; protected set; }

        public CarFacade CarFacade;

        public Car(BodyType bodyType)
        {
            Body = bodyType;

            Suspension suspension = new Suspension();
            Tires tires = new Tires();
            Engine engine = new Engine();
            carParts = new List<CarPart> { suspension, tires, engine };

            CarControll carControll = new CarControll();
            CarMovement carMovement = new CarMovement();
            CarDamage carDamage = new CarDamage(carParts);
            CarDiagnostic carDiagnostic = new CarDiagnostic(carParts);
            CarFacade = new CarFacade(carControll, carMovement, carDamage, carDiagnostic);
        }
        public void SetNewOwner(string owner)
        {
            OwnerName = owner;
        }

        List<CarPart> carParts;
    }
    public class Ford : Car
    {
        public Ford(string model, BodyType body) : base(body)
        {
            CarModel = model;
        }
    }
    public class Fiat : Car
    {
        public Fiat(string model, BodyType body) : base(body)
        {
            CarModel = model;
        }
    }

    //Facade Realization
    public class CarFacade
    {
        private CarControll carControll;
        private CarMovement carMovement;
        private CarDamage carDamage;
        private CarDiagnostic carDiagnostic;
        public CarFacade(CarControll controll, CarMovement movement, CarDamage damage, CarDiagnostic diagnostic)
        {
            carControll = controll;
            carMovement = movement;
            carDamage = damage;
            carDiagnostic = diagnostic;

            carControll.Attach(carDamage);
        }

        public void Gas()
        {
            carMovement.MoveForward();
            carControll.StepOnGas();
        }

        public void Break()
        {
            carMovement.MoveBackward();
            carControll.HitBreak();
        }

        public List<CarPart> Diagnostic()
        {
            return carDiagnostic.Diagnostic();
        }

        public void Repair(List<CarPart> partsForRepair)
        {
            foreach (var part in partsForRepair)
                part.Repair();
        }
    }
    public class CarControll : ICarDamageSubject
    {
        private List<ICarDamageObserver> observers = new List<ICarDamageObserver>();

        public void Attach(ICarDamageObserver observer)
        {
            this.observers.Add(observer);
        }

        public void Detach(ICarDamageObserver observer)
        {
            this.observers.Remove(observer);
        }

        public void HitBreak()
        {
            Console.WriteLine("Hit the Break");
            DamageNotify(DamageType.Suspension);
        }

        public void StepOnGas()
        {
            Console.WriteLine("Step On The Gas");
            DamageNotify(DamageType.Tires);
        }

        public void ChangeGear()
        {
            Console.WriteLine("ChangeGear");
        }

        public void TurnRudder(float angle)
        {
            Console.WriteLine($"Turn The Rudder on angle: {angle}");
        }

        public void DamageNotify(DamageType damageType)
        {
            foreach (var observer in observers)
            {
                observer.HandleNotify(damageType);
            }
        }
    }
    public class CarMovement
    {
        public void MoveForward()
        {
            Console.WriteLine("Move Forward");
        }

        public void MoveBackward()
        {
            Console.WriteLine("Move Backward");
        }
    }
    //Observer Realization
    public class CarDamage : ICarDamageObserver
    {
        private List<CarPart> carParts;
        public CarDamage(List<CarPart> carParts)
        {
            this.carParts = carParts;
        }

        public void HandleNotify(DamageType damageType)
        {
            foreach(var part in carParts)
            {
                if (part.DamageType == damageType)
                    part.DamagePart(60);
            }
        }
    }
    public class CarDiagnostic
    {
        private List<CarPart> repairableParts;
        public CarDiagnostic(List<CarPart> repairableParts)
        {
            this.repairableParts = repairableParts;
        }

        public List<CarPart> Diagnostic()
        {
            Console.WriteLine("Diagnostic");
            return repairableParts.Where(part => part.IsNeedRepair).ToList();
        }
    }
    //Needed only to confirm the purchase
    public interface ICarOwner
    {
        string OwnerName { get; }
        void SetNewOwner(string newOwner);
    }

    //Basic class for all car parts
    public abstract class CarPart : IRepairable
    {
        public int Condition { get; private set; }
        public bool IsNeedRepair => Condition < 50;
        public DamageType DamageType { get; protected set; }
        public CarPart()
        {
            Condition = 100;
        }

        public virtual void Repair()
        {
            Condition = 100;
        }

        public virtual void DamagePart(int damage)
        {
            Condition -= damage;

            if (Condition < 0)
                Condition = 0;
        }
    }

    public class Suspension : CarPart
    {
        public Suspension()
        {
            DamageType = DamageType.Suspension;
        }
        public override void DamagePart(int damage)
        {
            base.DamagePart(damage);
            Console.WriteLine($"Damage Suspension: {damage}");
        }

        public override void Repair()
        {
            base.Repair();
            Console.WriteLine("Repair Suspension");
        }
    }

    public class Tires : CarPart
    {
        public Tires()
        {
            DamageType = DamageType.Tires;
        }
        public override void DamagePart(int damage)
        {
            base.DamagePart(damage);
            Console.WriteLine($"Damage Tires: {damage}");
        }

        public override void Repair()
        {
            base.Repair();
            Console.WriteLine("Repair Tires");
        }
    }

    public class Engine : CarPart
    {
        public Engine()
        {
            DamageType = DamageType.Engine;
        }
        public override void DamagePart(int damage)
        {
            base.DamagePart(damage);
            Console.WriteLine($"Damage Engine: {damage}");
        }

        public override void Repair()
        {
            base.Repair();
            Console.WriteLine("Repair Engine");
        }
    }


    public enum BodyType
    {
        Cabriolet,
        Hatchback,
        Sedan
    }
    public enum DamageType
    {
        Tires,
        Suspension,
        Engine
    }
    //Responsible for the condition and possibility of repair of the part
    public interface IRepairable
    {
        int Condition { get; }
        DamageType DamageType { get; }
        bool IsNeedRepair { get; }
        void Repair();
    }

    public interface ICarDamageObserver
    {
        void HandleNotify(DamageType damageType);
    }

    public interface ICarDamageSubject
    {
        // Subscription
        void Attach(ICarDamageObserver observer);
        // Unsubscribe
        void Detach(ICarDamageObserver observer);
        // Notification
        void DamageNotify(DamageType damageType);
    }
}
