Feature: Logout flow

  Scenario: User attempts logout from screening page
    Given I am signed in as "onepathway" with password "Password123"
    When I click the button "Log out"
    Then I should be redirected to the page "/"
    And I should not have the session cookie

  Scenario: User attempts logout from breast screening page
    Given I am signed in as "onepathway" with password "Password123"
    And I go to the page "/breast-screening"
    When I click the button "Log out"
    Then I should be redirected to the page "/"
    And I should not have the session cookie

  Scenario: User attempts logout from cervical screening page
    Given I am signed in as "twopathway" with password "Password123"
    And I go to the page "/cervical-screening"
    When I click the button "Log out"
    Then I should be redirected to the page "/"
    And I should not have the session cookie

  Scenario: User attempts logout from bowel screening page
    Given I am signed in as "twopathway" with password "Password123"
    And I go to the page "/bowel-screening"
    When I click the button "Log out"
    Then I should be redirected to the page "/"
    And I should not have the session cookie
