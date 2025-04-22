Feature: Screening page

  Scenario: One pathway user logs in
    Given I am signed in as "onepathway" with password "Password123"
    Then I should see the heading "My screening"
    And I should see 1 clickable cards
    And I should see a card with title "Breast Screening"
    And I should see the user name "One" in the header


  Scenario: Two pathways user logs in
    Given I am signed in as "twopathway" with password "Password123"
    Then I should see the heading "My screening"
    And I should see 2 clickable cards
    And I should see a card with title "Cervical Screening"
    And I should see a card with title "Bowel Screening"
    And I should see the user name "Two" in the header
