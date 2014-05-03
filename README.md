Known Bugs and Issues:

	– When users exit the view of the Kinect too rapidly their hands remain behind, free-floating and stuck to where the Kinect lost sight of the user. Stepping back into frame does not correct the issue.

	– Color correction is too sensitive and often causes colors to become washed out or excessively bright.

	– Buttons are in-sensitive and require the user to hover their hand directly over the center of the button. This is suspected to be caused by the radius of the user's hands being calculated too conservatively.

	– Boids gravitate to the user's hands too strongly.

	– The exhibit does not do a good job of ignoring users in the distance. Likewise does not require a minimum amount of time in front of the exhibit before allowing the user to interact.

	– Boids sometimes overlap. Overlap behavior exists in codebase but is turned off as it often relocates offending boids to a location quite distant from where the overlap occured.
