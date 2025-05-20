Feature: I can see the cookies policy page

    Scenario: Check for page heading
        When I go to the page "/cookies-policy"
        Then I see the heading "Cookie policy"
