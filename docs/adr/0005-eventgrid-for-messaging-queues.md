# 5. Eventgrid for messaging queues

Date: 2025-03-27

## Status

Accepted

## Context

There are multiple queueing solutions within Azure. We need one that can handle high volume and support multiple subscribers to a particular topic

## Decision

Use EventGrid as the queueing solution

## Consequences

The eventhandlers will listen and respond to topics on the eventgrid
