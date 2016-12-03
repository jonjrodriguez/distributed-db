# Distributed Database

### Featuring:
* Multiversion Concurrency Control
* Deadlock Detection
* Replication
* Failure Recovery

### Algorithms:
* Available Copies
* Two Phase Locking
* Multiversion Read Consistency
* Validation at Commit
* Abort Youngest in Cycle

### Two ways to run:
1. __File Based:__ provide a path to a file
2. __Interactive:__ Do not include a file
  * Commands will be executed as they are entered
  * Type `exit` to finish

#### Commands:
* __Portable:__ `dotnet run -p src <file>`
* __Standalone:__ 

### Available operations:
* begin(T1): begins a transaction named T1
* beginRO(T1): begins a read-only transaction named T1
* R(T1, x1): transaction T1 attempts to read variable x1
* W(T1, x1, 100): transaction T1 attempts to write variable x1 with value 100
* end(T1): transaction T1 is either committed or aborted
* fail(1): site 1 fails
* recover(1): site 1 recovers
* dump(): shows the current state of all variables at all sites
* dump(i): shows the current state of all variables at site i
* dump(xj): shows the current state of variable xj at all sites

### Built With:
* .NET Core 1.1