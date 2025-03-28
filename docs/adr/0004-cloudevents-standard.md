# 4. CloudEvents standard

Date: 2025-03-27

## Status

Accepted

## Context

There are two available data standards supported by Azure's Eventgrid. EventData and CloudEvents. The EventData is a Microsoft standard, but CloudEvents is open standard. Whilst the main bulk of the NSP will reside within Azure, there is the expectation that data will need to flow from external sources. Picking a more open standard would make this easier

## Decision

Adoption of CloudEvents for our event messaging

## Consequences

Can provide much smoother external integration.
