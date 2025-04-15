Feature: I can see the cookies policy page

    Scenario: Check for page heading
        Given I am on the page "/cookies-policy"
        When I arrive on on the page "/cookies-policy"
        Then I see the heading "Cookie policy"
