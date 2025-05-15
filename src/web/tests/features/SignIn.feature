Feature: Sign In Process

    Scenario: I can sign in successfully with a valid user
        When I go to the page "/"
        When I click the button "Continue to NHS login"
        And I fill in the login form with username "onepathway" and password "Password123"
        Then I should be redirected to the page "/screening"
        And I should see the heading "My screening"

    Scenario: I cannot sign in with a P5 identity proofing level user
        When I go to the page "/"
        When I click the button "Continue to NHS login"
        And I fill in the login form with username "peafive" and password "Password123"
        Then I should be redirected to the page "/error/auth"
        And I should see the heading "Authentication error"



