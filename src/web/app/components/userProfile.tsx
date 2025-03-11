import SignOutButton from "@/app/components/signOutButton";

interface UserProfileProps {
  firstName?: string;
  lastName?: string;
  nhsNumber?: string;
}

export default async function UserProfile({
  firstName,
  lastName,
  nhsNumber,
}: UserProfileProps) {
  return (
    <>
      <hr />
      <p>
        Logged in as {firstName} {lastName} ({nhsNumber})
      </p>
      <SignOutButton />
    </>
  );
}
