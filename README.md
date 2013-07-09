Repository
=============

Repository is a generic implementation of the Repository pattern in C#. It provides a repository base class ```Repository<T>``` that exposes functions to store/retrieve data,
and an object context base class ```ObjectContext<T>``` that enables manipulation of data once it's retrieved. It also exposes an ```Items``` property that returns 
an ```IQueryable<T>``` which can be used to perform LINQ queries on the repository.

Implementations
================

This (code) repository comes with a few Repository implementations. The first and simplest is ```InMemoryRepository```, which acts as a temporary in-memory store useful mainly for testing. 
The second is an Entity Framework repository named ```EFRepository```, which uses [Entity Framework](http://msdn.microsoft.com/en-us/data/ef.aspx) as its 
storage interface. The thid is ```AzureRepository```, an implementation of ```Repository``` for [Azure Blob Storage](http://www.windowsazure.com/en-us/manage/services/storage/).
And the fourth is ```FileSystemRepository```, an implementation that serializes objects directly to the filesystem.


Pull requests for additional implementations (and improvements to existing ones) are welcome and encouraged.

Change Tracking
=================

As you'll see in the examples below, changes to objects accessed via a ```Repository``` instance are tracked automatically and committed back to the underlying data store whenever
a call to ```SaveChanges``` is made. This functionality is enabled automatically for any class which derives from ```Repository```; if you're writing a new implementation, you'll get
change tracking for free. See the 'Modify an object' example below to get an idea for how this works.

Examples
===========

Store an object:
-----------------

```C#
var repository = new MyConcreteRepository<int>();
repository.Insert(1); // Store a single value
repository.SaveChanges();


repository.Insert(new[] { 2, 3, 4, 5 }); // Store a bunch of values
repository.SaveChanges();
```

Retrieve an object:
--------------------

```C#
class MyClass 
{
	public String Key { get; set; }
}

/* ... */

using (var repository = new MyConcreteRepository<MyClass>())
{
	var objectContext = repository.Find("myKey");
	var obj = objectContext.Object;

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

using (var repository = new MyConcreteRepository<MyClass>())
{
	// Query objects based on their Value field
	var filteredItems = repository.Items.Where(x => x.Value > 5);

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

using (var repository = new MyConcreteRepository<MyClass>())
{
	repository.RemoveByKey("myKey");
	repository.SaveChanges();

	// Or...

	var obj = new MyClass { Key = "myKey" };
	repository.Remove(obj);
	repository.SaveChanges();

	// Or...

	repository.RemoveByKey("myKey");
	repository.SaveChanges();

	// Or...

	repository.RemoveAllByKey("myKey");
	repository.SaveChanges();

	// Or...
	repository.RemoveAll();
	repository.SaveChanges();
}
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
using (var repository = new MyConcreteRepository<MyClass>())
{
	// Pull the object from the repository
	var valueContext = repository.Find("myKey");

	// Modify it
	valueContext.Object.Value = 5;

	// Save the changes
	repository.SaveChanges();
}
```


Typed Key Repositories
==========================
Stored objects are accessed by their keys, as in the examples above. Normally, keys are passed into ```Repository``` methods as untyped ```params``` arguments; as a result,
the compiler will not perform any checks for either the number or types of keys passed in. This allows for flexibility in the types of keys you can use,
but also negates some of the benefits of having a strongly-typed generic repository in the first place. 

In order to fix this potential problem, this library contains a series of ```Repository``` base classes that take type parameters for keys. These classes
simply wrap the "untyped" repositories with strongly-typed versions of certain methods, like ```Find``` and ```RemoveByKey```. In order to make typed versions of
your repositories available, simply derive from the strongly-typed versions of ```Repository``` and pass in instances of your untyped repositories to the base
constructor.

For example, ```EFRepository``` exposes strongly-typed versions (for classes with up to two key values) like so:


```C#
public class EFRepository<TContext, TValue, TKey> : Repository<TValue, TKey> where TValue : class where TContext : DbContext
{
    //===============================================================
    public EFRepository(Func<TContext, DbSet<TValue>> setSelector, TContext context = null)
        : base(new EFRepository<TContext, TValue>(setSelector, context))
    {}
    //===============================================================
}

public class EFRepository<TContext, TValue, TKey1, TKey2> : Repository<TValue, TKey1, TKey2>
    where TValue : class
    where TContext : DbContext
{
    //===============================================================
    public EFRepository(Func<TContext, DbSet<TValue>> setSelector, TContext context = null)
        : base(new EFRepository<TContext, TValue>(setSelector, context))
    { }
    //===============================================================
}
```

All these classes do is instantiate untyped instances of ```EFRepository``` and pass them into the base ```Repository``` constructor. Once instantiated,
these strongly-typed versions of ```EFRepository``` will enforce type safety on all key lookups. You can see this by looking at methods that make use of keys. For
example, the signature of ```Find``` in the strongly-typed version of ```Repository``` looks like this:

```C#
public ObjectContext<TValue> Find(TKey key)
{
    return InnerRepository.Find(key);
}
```

Notice how, instead of ```params Object[]```, the sole argument here is of type ```TKey```. This will allow the compiler to check for invalid lookups at compile-time,
which can be very handy.

EFRepository 
==============

In order to create an instance of ```EFRepository```, you need to first declare a derived class of 
[DbContext](http://msdn.microsoft.com/en-us/library/gg679505.aspx)
with ```DbSet``` instances for all of the types for which you want to have repositories. An example ```DbContext``` might look like this:

```C#
class TestClass
{
	[Key]
	public String Key { get; set; }
}

/* ... */

class MyContext : DbContext
{
	public DbSet<TestClass> TestClasses { get; set; }
}
```

Once a subclass of ```DbContext``` is defined, you can instantiate an ```EFRepository``` for type ```TestClass``` like so:

```C#
var repository = new EFRepository<MyContext, TestClass>(x => x.TestClasses);
```

The sole argument to the constructor is a ```Func<MyContext, TestClass>``` that tells the repository how to find the appropriate ```DbSet``` in the ```DbContext```.

Note that the procedures normally followed when using Entity Framework must still be followed here; that is, you should still specify connection strings in
your ```Web.config``` and decorate your key properties with ```[Key]``` attributes if necessary.

AzureRepository
===============

All you need for ```AzureRepository``` is a proper connection string for [Azure blob storage](http://www.windowsazure.com/en-us/develop/net/how-to-guides/blob-storage/). 
You can either pass this connection string directly to an instance of ```AzureRepository```, or you can store it in a App.config/Web.config file and reference it by name.
There's also a static ```ForStorageEmulator``` method that will create an instance of ```AzureRepository``` pointed at the local 
[Azure storage emulator](http://msdn.microsoft.com/en-us/library/windowsazure/ff683674.aspx).

Re-using the ```TestClass``` class from above, instantiating an ```AzureRepository``` looks like this:

```C#
// From explicit connection string
var repository = AzureRepository<TestClass>.FromExplicitConnectionString(x => x.Key, myConnectionString);

// From named connection string
var repository = AzureRepository<TestClass>.FromNamedConnectionString(x => x.Key, "myConnectionStringName");

// For storage emulator
var repository = AzureRepository<TestClass>.ForStorageEmulator(x => x.Key);
```

There is also an ```AzureOptions``` class that allows you to specify how e.g. objects are serialized to blob storage, access rules for stored objects, content type, etc.

FileSystemRepository
=====================

```FileSystemRepository``` is an implementation that (surprise) saves objects to the local filesystem. Instantiating one is very simple:

```C#
var repository = new FileSystemRepository<TestClass>(x => x.Key);
```

 You can also optionally specify a ```FileSystemOptions``` parameter that configures how objects are serialized to disk, where they're stored, the file extension, etc.

Implementing Your Own Repositories
================================

To understand how to go about doing implementing your own repositories, we need to 
briefly go over how repositories work under the hood.

All operations on ```Repository``` implementations are deferred; that is, they are not committed until a call to
```SaveChanges``` is made. This is achieved by storing a list of pending operations and applying them 
sequentially when ```SaveChanges``` is called. These pending operations are objects which implement the
interface ```Operation```, which has a single method called ```Apply```:

```C#
public interface Operation
{
    //===============================================================
    void Apply();
    //===============================================================
} 
```

There are three main types of operations: ```Insert```, ```Remove```, and ```Modify```. 
These are abstract base classes. Each ```Repository``` implementation will provide its own concrete versions
of these classes which perform the necessary operations. As an example, the abstract ```Insert``` class looks
like this:

```C#
public abstract class Insert<TValue> : Operation
{
    //===============================================================
    public Insert(IEnumerable<Object> keys, TValue value)
    {
        Keys = keys;
        Value = value;
    }
    //===============================================================
    public IEnumerable<Object> Keys { get; private set; }
    //===============================================================
    public TValue Value { get; private set; }
    //===============================================================
    public abstract void Apply();
    //===============================================================
}
```

Notice that this base class stores all of the information necessary to perform an insert into some underlying data
store, namely the object to be inserted and the keys under which it should be stored.

Now take a look at an implementation of ```Insert```, this one for ```InMemoryRepository```:

```C#
internal class FileSystemInsert<T> : Insert<T>
{
    //===============================================================
    public FileSystemInsert(IEnumerable<object> keys, T value, FileSystemInterface<T> fsInterface)
        : base(keys, value)
    {
        FileSystemInterface = fsInterface;
    }
    //===============================================================
    private FileSystemInterface<T> FileSystemInterface { get; set; }
    //===============================================================
    public override void Apply()
    {
        FileSystemInterface.StoreObject(Value, Keys);
    }
    //===============================================================
}
```

All this class does in its ```Apply``` method is call a supplied ```FileSystemInterface``` object to perform
the actual insert. The ```FileSystemInterface``` class is a utility class that handles filesystem manipulation,
but you could just as easily implement the filesystem serialization logic directly in ```Apply```. The point is 
that the implementation of ```Insert``` is responsible for initiating the actual insertion operation into the 
underlying data store. 

In order to implement your own repositories, you have to implement three methods from the base ```Repository```
class which correspond to the three types of operations mentioned above: 

```C#
//===============================================================
protected abstract Insert<T> CreateInsert(IEnumerable<object> keys, T value);
//===============================================================
protected abstract Remove CreateRemove(IEnumerable<object> keys);
//===============================================================
protected abstract Modify<T> CreateModify(IEnumerable<object> keys, T value, Action<T> modifier);
//===============================================================
```

These methods are responsible for returning implementations of the three types of operations, as shown
above. 

In addition to these methods, you must also implement three others:

```C#
//===============================================================
protected abstract ObjectContext<T> FindImpl(object[] keys);
//===============================================================
public abstract bool ExistsByKey(params Object[] keys);
//===============================================================
public abstract EnumerableObjectContext<T> Items { get; } 
//===============================================================
```

```FindImpl``` is reponsible for looking up an object from the underlying data store,
```ExistsByKey``` is responsible for checking for an object's existence by its key values,
and ```Items``` is responsible for enumerating the objects in the repository. These will likely
call methods directly on the underlying data store or make use of some sort of interface such as 
```FileSystemInterface``` (mentioned above). Again, the point is that these methods are responsible for
interfacing with the underlying data store. As an example, if you were implementing a repository for Amazon S3 
storage, the FindImpl method would likely call some sort of lookup method in the S3 API.


License
===========

Repository is licensed under the New BSD License:

```
Copyright (c) 2013, Matthew Schrager
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name of Matthew Schrager nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL MATTHEW SCHRAGER BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```









