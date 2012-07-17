Repository
=============

Repository is a generic implementation of the Repository pattern in C#. It provides a repository interface ```IRepository<T>``` that exposes functions to store/retrieve data,
and an object context interface ```IObjectContext<T>``` that enables manipulation of data once it's retrieved. It also exposes a ```GetItemsContext``` method that returns
an ```IQueryable<T>``` which can be used to perform LINQ queries on the repository.

Examples
===========

Store an object:
-----------------

```C#
var repository = new MyConcreteRepository<int>();
repository.Store(1); // Store a single value
repository.Store(new[] { 1, 2, 3, 4, 5 }); // Store a bunch of values
```

Retrieve an object:
--------------------

```C#
class MyClass 
{
	public String Key { get; set; }
}

/* ... */

var repository = new MyConcreteRepository<MyClass>();

// Objects are returned inside of their own ObjectContexts, which should be disposed
using (var valueContext = repository.Find("myKey"))
{ 
	var value = valueContext.Object;

	// Do cool things with this value
}
```

Query stored objects:
----------------------

```C#
class MyClass
{
	public String Key { get; set; }
	public int Value { get; set; }
}

/* ... */

var repository = new MyConcreteRepository<MyClass>();
using (var itemsContext = repository.GetItemsContext())
{
	// Query objects based on their Value field
	var filteredItems = itemsContext.Where(x => x.Value > 5);

	// Do stuff with query result
}
```


Remove an object:
-----------------

```C#
class MyClass 
{
	public String Key { get; set; }
}

/* ... */

var repository = new MyConcreteRepository<MyClass>();
repository.Remove("myKey");
```

Modify a stored object:
-----------------------

```C#
class MyClass 
{
	public String Key { get; set; }
	public int Value { get; set; }
}

/* ... */

// Pull the object from the repository
var repository = new MyConcreteRepository<MyClass>();
using (var valueContext = repository.Find("myKey"))
{
	// Modify it
	valueContext.Object.Value = 5;

	// Save the changes
	valueContext.SaveChanges();
}
```









