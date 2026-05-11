# Squad Commands and Prompts

## Start
```
I'm starting a new project. Set up the team based on the Firefly universe.
Here's what I'm building: please review the docs/prd-elevator-dispatch.md file for the product requirements.
```

## Test 1
```
 can you fire it up so I can test it?
```

## Revision 1
```
 This seems to run pretty good, but the UI could use some refinement though.
 The blazor UI still has the left menu  which has weather and counter which should be removed;
 the title bar is empty and we have another title bar in the app window;
 and the UI is pretty bland and could be modernized.
 can you give that a refresh?
```

## Revision 2
```
the UI seems a little out of sync -- the elevator status should show up under the elevator, shouldn't it?  (the image attached shows they are in different columns)
```
 
## Query
```
how does the test coverage look?  is that good now?  > 80% coverage?
  --> Coverage is currently below target. Line coverage is 27.86% (branch coverage 32.55%), so it is not > 80% yet.
so - tell the team to get busy and raise that to 80%!
  --> Mal and Simon are done too and confirm coverage at 95.12%, so the 80% goal is clearly met.
```

## Deploy 1
```
Next up -- review exactly how the app is deployed in the golden repo at https://github.com/lluppesms/dadabase.demo.  Set up a bicep deploy and GitHub Actions, replicating the golden repo deployment as closely as you can. Don't make up new patterns - just reuse the patterns you find in the golden repo and adjust as necessary.
```

## Deploy 2
```
can you also have Zoe clone the "azd up" functionality in the golden repo?  I'd like to add that here
```

## Deploy 3
```
the azd up is not working.  in order for it to be successful, it needs to set these environment variables that the main.bicepparm file is looking for.   Can you update the azd config to request these from the user or set them appropriately?
- appName #{APP_NAME}#
- deploymentType #{DEPLOYMENT_TYPE}#
- environmentCode #{ENVCODE}#
- instanceNumber #{INSTANCE_NUMBER}#
- location #{RESOURCE_GROUP_LOCATION}#
```
