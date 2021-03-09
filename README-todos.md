# TODOs

## Fix Known Bugs

- _None reported currently._
- _All software has bugs; please file & track issues._ 

## Consider and/or Implement Suggested Improvements

1. **Improve Online "Editor" Help**
   - _**Background:**_
     - Pressing "F1" or invoking the "Help" menu in the editor pops up
       a simple list of the substitution tokens available for standard
       lens templates.  This popup is invoked even when one of the sub-
       dialogs is open (such as "Rename Lens", "Load from File", or
       "Save to File", etc.)
   - _**Suggested Improvements:**_
     1. Since access to the context (substitution) values differs based
        upon which template language is used, the existing help is often
        not useful, and even misleading.  Either it should be context-
        sensitive so that it reflects how accessing the context values
        based upon the language of the current lens template, or should
        have more general coverage of the template languages, and how
        to access context values in each.
     2. When one of the sub-dialogs is open (e.g., "Rename", "Save/Load
        As...") pressing "F1" (or accessing a "Help" menu which could
        be added) should provide context-appropriate help, i.e., not
        the same help that's provided in the lens editor itself.
2. **Write an example LiveCam using C#**
   - _**Background**_
     - There are three lens templating languages available (text,
       Razor Page, and Roslyn/C#).  The two LiveCams provided in the
       distribution demonstrate use of the text and Razor Page types,
       but there is no example provided of using a C# template.
   - _**Suggested Solution:**_
     - A LiveCam should be created that demonstrates how a C# template
       could be used to justify its existence, specifically suggesting
       case(s) where it would be the preferred templating language 
       based upon its ability to better access APIs or other features
       which would be more difficult or even impossible using one of
       the other templating languages.
3. **Allow user to specify initial parameters for "RandomWalk" Connector**
   - _**Background:**_
     - The initial position (latitude, longitude, altidude, bank angle,
       tilt, rate of change of each, ...) is "hard-coded" in the
       implementation of the "RandomWalk" connector.  Therefore, the
       variability within the "random flight" produced is very small.
     - It would be nice if the user could specify the starting position
       and the amount of variability (i.e., the "squirreliness") of the
       "random flight" to be produced.
   - _**Suggested Improvement:**_
     - Add a popup dialog to collect these (and possibly other) 
       parameter values when a "Random Walk" connector is started
       (e.g., after the user presses the "Connect" button when
       "RandomWalk" is selected as the current "Connector"), and
       employ the user-provided values instead of the hard-coded
       values now used.
4. **Use a temporary file for `Launch KML File`**
   - _**Background:**_
     - The code triggered when the `Launch KML File` button is pressed
       is the same as that triggered when the `Create KML File`
       button is pressed.  Therefore, a file is created with the same
       name in the same place in both cases.  It's likely that the
       user does not care to see the (intermediate) file created when
       the `Launch KML File` button is pressed.
   - _**Suggested Improvement:**_
     - The (intermediate) file created when the `Launch KML File` 
       button is pressed could be written to a temporary folder, and/or
       perhaps deleted after being sent to Google Earth and is therefore
       no longer needed.
5. **Simplify Installation and Update - (e.g., use "ClickOnce"?)**
    - _**Background:**_
      - Though simple for the producer, the approach of releasing the
        application in an unsigned zip file creates several significant
        disadvantages for the end user.  It's:
        1. labor-intensive - requiring the user to download, unzip, and
           directly run the included executable file, which must be searched
           for, found, and then invoked - all of which requires a level of 
           technical savvy that not all potential users possess.
        1. scary - because the application is unsigned, the operating system
           challenges the user to think twice and makes it hard to run it,
           making it clear that it could contain viruses or other malware.
           This also will reduce the potential audience of the application.
      - Several attempts to configure "ClickOnce" installations have failed,
        mainly due to the (apparent) complexity of this configuration,
        publication or management of the involved "secrets" (such as the
        signing certificate, etc.), etc.
    - _**Suggested Improvement:**_
      - Choose and employ (at least) one of the standard installation procedures
        available for Windows Forms applications which address the shortcomings
        enumerated above.
      - It appears that "ClickOnce" could be used to facilitate both easy
        installation and (check for) upgrade from the GitHub website.
      - Completion criteria: choose and implement the improved installation
        procedure, and update relevant portion(s) of the documentation for
        consistency.
