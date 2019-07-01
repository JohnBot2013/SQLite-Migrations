# SQLite-Migrations
## Manage Database changes in your app with SQLite

This utility depends on the wonderful SQLite library [SQLite-net](https://github.com/praeclarum/sqlite-net) by Frank A. Krueger.

Any database application needs a way to maintain its schema. With Entity Framework you have a "Migrations" tool built in to Visual Studio. With SQLite then there is not a "built in" way to maintain your database schema with changes tied to the application version.

This utility provides a way to keep your database in line with your code base automatically, every time your application starts.

Each set of changes is captured using an IMigration. Each Migration must have a 3 digit sequential number at the end of its name. When each IMigration is executed, the number becomes the current application version and is stored using a SQLite pragma.

The Migration process is very efficient, Migrations are only executed if their number is higher than the current version.

Each Migration can provide as complex a set of changes as needed and includes seeding.

The utility provides a BaseMigration for you to use instead of implementing your own IMigration. This has a number of utilities which are helpful when making table or data changes.

The unit tests provide some examples of usage.

In your application you need to initialise DbInitialiser with an SQLiteConnection and the assembly which contains your IMigration classes.

You can then run dbInitialiser.InitialiseDatabase() to peform a completely automatic migration or you can provide DbInitialiser with an array of pre-configured and sorted IMigration objects. The unit tests illustrate how to use both methods.
