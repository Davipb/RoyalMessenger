![Logo](Logo.svg)

## Adaptible & decoupled message broker

## What it does
**Royal Messenger** is a library that lets different parts of your system communicate without having to
reference one another directly.
Anyone can send a message, anyone can receive a message, and once a component goes out of scope and is
garbage-collected, it automatically stops receiving messages, without the senders having to ever know that 
happened.

## Why use it
As a codebase grows, so does the need to move information between components.
This ends up causing dependencies between objects that don't need each-other to function, just for the sake
or reporting some new data or describing a change.

With time, either constructors with 5+ non-functional dependencies sprout everywhere, or one monolithic 
"information center" grows to reference almost every object in the project.
This doesn't affect just the maintainability of code, but can also cause runtime memory hogs, as objects
inevitably end up referencing a "dead" object and preventing Garbage Collection.

With Royal Messenger, you replace all your non-functional dependencies with a single reference to the
long-lived Messenger object, then subscribe to the message you want to hear and send messages when you
have new data to report. Once your object is no longer needed, it is garbage-collected normally, and stops
receiving messages without affecting others using the messenger.
